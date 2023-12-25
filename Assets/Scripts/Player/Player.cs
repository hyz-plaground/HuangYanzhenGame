using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


public class Player : MonoBehaviour
{
    
    [SerializeField]
    // Debug Settings
    public bool showDebugMessage = false;
    
    // Player's main rigid body
    private Rigidbody2D _rigid;

    // Player Movement
    public float moveSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_MOVE_SPEED;
    public float jumpSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_JUMP_SPEED;
    public float rushSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_SPEED;
    public string groundTag = PlayerProperties.Instance.PLAYER_GROUND_TAG;         // Ground Detection
    
    // Static Player Properties
    public float groundDetectionRaycastDistance = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RACAST_DISTANCE;
    public float rushCD = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_CD;
    public bool haveRushCD = false;
    public float maxGravityScale = PlayerProperties.Instance.PLAYER_MAX_GRAVITY_SCALE;
    public int maxLife = PlayerProperties.Instance.PLAYER_DEFAULT_MAX_LIFE;
    public float fallDamageSpeedThreshold = PlayerProperties.Instance.PLAYER_FALL_DAMAGE_THRESHOLD;

    // Dynamic Player variables.
    private bool _isFaceTowardsRight = true;
    private bool _isSpaceGravity = false;
    private bool _isOnGround;
    private float _rushTimeInterval;
    private int _currentLife;
    
    // Delegates

    // Initialize parameters.
    void InitParams()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _rushTimeInterval = rushCD;     // Full rush interval, allowing player to rush.
        _isFaceTowardsRight = true;     // Initialize player face to right.
        _isSpaceGravity = false;        // Initialize player gravity.
        _currentLife = maxLife;         // Maximizes player life.
    }

    // Initialize event listeners.
    void InitDelegates()
    {
        EventCenterManager.Instance.AddEventListener<bool>(GameEvent.PlayerEnterSpace, TriggerSpaceGravityRegion);
        EventCenterManager.Instance.AddEventListener<int>(GameEvent.PlayerGetHurt, GetHurt);
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerDie,Die);
    }
    
    void Start()
    {
        InitParams();       // Initialize player properties.
        InitDelegates();    // Initialize event listeners.
    }
    
    void Update()
    {
        Move();
        Timer();
        AdjustPropertyConstantly();
        PassiveSenseEnvironment();
    }
    
    #region Movement
    
    /* Player movements.*/
    private void Move()
    {
        /* Get WASD inputs */
        // TODO: Get these inputs into configuration file.
        float xInput = Input.GetAxis("Horizontal");
        bool jumpInput = Input.GetKey(KeyCode.Space);
        bool flyInput = Input.GetKeyDown(KeyCode.Space);
        bool rushInput = Input.GetKey(KeyCode.R);
        bool interactInput = Input.GetKey(KeyCode.F);
        
        // Control Facing Direction
        _isFaceTowardsRight = xInput != 0 ? xInput >= 0 : _isFaceTowardsRight;
        
        /* Calculate player movements based on inputs.*/
        // Horizontal Movements
        float xMove = 0;
        if (xInput != 0)
        {
            xMove = CheckIntendRush(rushInput) ? xInput * rushSpeed : xInput * moveSpeed;
        }
        else if (CheckIntendRush(rushInput))
        {
            xMove = _isFaceTowardsRight ? rushSpeed : -rushSpeed;
        }
        
        // Vertical Movements
        float jumpMove;
        if (CheckAllowFly())
        {
            jumpMove = flyInput? moveSpeed : _rigid.velocity.y - 10f * Time.deltaTime;
        }
        else
        {
            jumpMove = CheckAllowJump() && jumpInput? jumpSpeed : _rigid.velocity.y;
        }
        
        /* Perform moving. */
        _rigid.velocity = new Vector2(xMove, jumpMove);
        
        /* Check player interaction. */
        Interact(interactInput);
        
    }
    
    /// <summary>
    /// Checks whether the player is allowed to jump. Box ray detect if on ground.
    /// </summary>
    /// <returns></returns>
    bool CheckAllowJump()
    {
        return CheckGround();
    }

    /// <summary>
    /// Checks whether the player is allowed to rush.
    /// <param name="isRushKeyPressed"> Input a key detection of rush key.</param>
    /// </summary>
    /// <returns> A boolean value noting if it is allowed to rush.</returns>
    bool CheckIntendRush(bool isRushKeyPressed)
    {
        bool isAfterRunCD = _rushTimeInterval >= rushCD;
        bool isAllowRush = (isAfterRunCD || !haveRushCD) && isRushKeyPressed;
        
        if(isAllowRush)
            _rushTimeInterval = 0;
        
        return isAllowRush;
    }
    
    /// <summary>
    /// Checks whether the player is allowed to fly.
    /// </summary>
    /// <returns></returns>
    bool CheckAllowFly()
    {
        return _isSpaceGravity;
    }
    
    /// <summary>
    /// Player interact with environment.
    /// </summary>
    /// <param name="isIntendInteract"></param>
    void Interact(bool isIntendInteract)
    {
        if (!isIntendInteract)
            return;
        EventCenterManager.Instance.EventTrigger(GameEvent.PlayerTryInteract);
    }

    #endregion

    #region Environment Awareness

    void PassiveSenseEnvironment()
    {
        return;
    }

    // Check whether the character is on ground. Use box ray cast.
    bool CheckGround()
    {
        // Ray detection part.
        // Box width align to player's width. It's center at lower of player.
        Vector2 boxSize = new Vector2(Math.Abs(transform.localScale.x), groundDetectionRaycastDistance);
        Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - 1.2f);
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, groundDetectionRaycastDistance);
        return hit.collider && hit.collider.CompareTag(groundTag);
    }
    

    #endregion
    
    #region Constant Property Adjustments

    void AdjustPropertyConstantly()
    {
        AdjustLocalScaleByFaceDir();
        AdjustGravityScale();
    }
    
   /// <summary>
   /// Adjust player face direction according to last speed dir.
   /// </summary>
    void AdjustLocalScaleByFaceDir()
    {
        float scale = _isFaceTowardsRight ? 1 : -1;
        transform.localScale = new Vector3(scale,1,1);
    }
    
    /// <summary>
    /// When player is not in space and vertical velocity is big enough, adjust gravity scale.
    /// </summary>
    void AdjustGravityScale()
    {
        // When in space gravity, this method don't work.
        // When vertical velocity is small, restore gravity scale.
        if (_isSpaceGravity || Math.Abs(_rigid.velocity.y) <= 0.1f)
        {
            _rigid.gravityScale = _isSpaceGravity? 0 : 1;
            return;
        }
        
        // When player is moving horizontally and didn't reach max grav scale
        // gradually increase gravity scale for player.
        if (Math.Abs(_rigid.velocity.y) >= 0.1f && _rigid.gravityScale <= maxGravityScale)
        {
            _rigid.gravityScale += Time.deltaTime * 10f;
        }
    }



    #endregion
    
    #region Ocassional Property Adjustments
    /// <summary>
    /// Adjust player running speed.
    /// </summary>
    /// <param name="speed"> The speed which you want the player to be at. Positive/Negative values
    /// will result in opposite moving directions.</param>
    void AdjustMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    
    void AdjustJumpSpeed(float speed)
    {
        jumpSpeed = speed;
    }

    // Trigger space gravity
    void TriggerSpaceGravityRegion(bool enter)
    {
        _isSpaceGravity = enter;
        _rigid.gravityScale = enter? 0 : 1;
    }

    /// <summary>
    /// Player get hurt.
    /// </summary>
    /// <param name="hurtVal">Number of life to deduct.</param>
    void GetHurt(int hurtVal)
    {
        if (_currentLife <= 0 || _currentLife - hurtVal <=0)
        {
            _currentLife = 0;
            Die();
            return;
        }
        _currentLife -= hurtVal;
    }

    /// <summary>
    /// Player dies.
    /// </summary>
    void Die()
    {
        //Debug.Log("Player dies.");
        //EventCenterManager.Instance.EventTrigger(GameEvent.PlayerDie);
    }
    
    
    
    #endregion
    
    #region Timer
    void Timer()
    {
        if(_rushTimeInterval < rushCD)
            _rushTimeInterval += Time.deltaTime;
    }
    #endregion
    
    #region Visualization
    // Draw the ray cast in Unity Editor.
    void OnDrawGizmos()
    {
        Vector2 boxSize = new Vector2(transform.localScale.x, groundDetectionRaycastDistance);
        Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - 1.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
    
    #endregion
}
