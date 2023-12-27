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
    public Collider2D playerCollider;
    
    // Player's main rigid body
    private Rigidbody2D _rigid;
    

    // Player Movement
    [Header("Basic Player Movement")]
    public float moveSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_MOVE_SPEED;
    public float jumpSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_JUMP_SPEED;
    public float rushSpeed = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_SPEED;
    
    [Header("Detailed Player Movement")]
    public float rushCD = PlayerProperties.Instance.PLAYER_DEFAULT_RUSH_CD;
    public bool haveRushCD = false;
    public float maxAllowCoyoteTime = PlayerProperties.Instance.PLAYER_MAX_ALLOW_COYOTE_TIME;   //离开地面后的最大土狼时间
    public float maxGravityScale = PlayerProperties.Instance.PLAYER_MAX_GRAVITY_SCALE;
    
    // Static Player Properties
    [Header("Ground Detection")]
    public string groundLayerMask = PlayerProperties.Instance.PLAYER_GROUND_LAYER_MASK;         // Ground Detection
    public float groundDetectionRaycastDistance = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE;
    public float raycastUpPosition = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_UP_POSITION;
    public float raycastMaxDistance = PlayerProperties.Instance.PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE;
    
    [Header("Player Life")]
    public int maxLife = PlayerProperties.Instance.PLAYER_DEFAULT_MAX_LIFE;
    public float fallDamageSpeedThreshold = PlayerProperties.Instance.PLAYER_FALL_DAMAGE_THRESHOLD;

    
    // Dynamic Player variables.
    private bool _isFaceTowardsRight = true;
    private bool _isSpaceGravity = false;
    private float _timeAfterLastRushed;
    private float _remainingAllowCoyoteTime;  //土狼
    private int _currentLife;
    
    // Delegates

    // Initialize parameters.
    void InitParams()
    {
        _rigid = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        _timeAfterLastRushed = rushCD;
        _remainingAllowCoyoteTime = 0;
        _isFaceTowardsRight = true;     // Initialize player face to right.
        _isSpaceGravity = false;        // Initialize player gravity.
        _currentLife = maxLife;         // Maximizes player life.
    }

    // Initialize event listeners.
    private void InitDelegates()
    {
        EventCenterManager.Instance.AddEventListener<bool>(GameEvent.PlayerEnterSpace, TriggerSpaceGravityRegion);
        EventCenterManager.Instance.AddEventListener<int>(GameEvent.PlayerGetHurt, GetHurt);
        EventCenterManager.Instance.AddEventListener(GameEvent.PlayerDie,Die);
    }
    
    private void Start()
    {
        InitParams();       // Initialize player properties.
        InitDelegates();    // Initialize event listeners.
    }
    
    private void Update()
    {
        Timer();
        Move();
        AdjustPropertyConstantly();
        //PassiveSenseEnvironment();
    }
    

    #region Movement
    
    /* Player movements.*/
    private void Move()
    {
        /* Get WASD inputs */
        // TODO: Get these inputs into configuration file.
        var xInput = Input.GetAxis("Horizontal");
        var jumpInput = Input.GetKey(KeyCode.Space);
        var flyInput = Input.GetKeyDown(KeyCode.Space);
        var rushInput = Input.GetKey(KeyCode.R);
        var interactInput = Input.GetKey(KeyCode.F);
        
        // Control Facing Direction
        _isFaceTowardsRight = xInput != 0 ? xInput >= 0 : _isFaceTowardsRight;
        
        /* Calculate player movements based on inputs.*/
        // Horizontal Movements
        var xMove = xInput * moveSpeed;
        
        if (rushInput && CheckAllowRush())
        {
            Debug.Log("Rush");
            xMove = _isFaceTowardsRight ? rushSpeed : -rushSpeed;
            _timeAfterLastRushed = 0;
        }
        
        // Vertical Movements
        float jumpMove;
        if (CheckAllowFly())
        {
            jumpMove = flyInput? moveSpeed : _rigid.velocity.y - 10f * Time.deltaTime;
        }
        else
        {
            TrySetAllowCoyote();
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
    private bool CheckAllowJump()
    {
        // return CheckGround();
        return ThreeHitGroundCheck() >= 1 || CheckAllowCoyote();
    }

    private bool CheckAllowCoyote()
    {
        if (ThreeHitGroundCheck() >= 1) // Player is on the ground, obviously not allow.
            return false;
        return _remainingAllowCoyoteTime > 0;
    }

    private void TrySetAllowCoyote()
    {
        if (ThreeHitGroundCheck() == 2)
        {
            // Player Jumps from the edge
            _remainingAllowCoyoteTime = maxAllowCoyoteTime;
        }
    }
    /// <summary>
    /// Checks whether the player is allowed to rush.
    /// <param name="isRushKeyPressed"> Input a key detection of rush key.</param>
    /// </summary>
    /// <returns> A boolean value noting if it is allowed to rush.</returns>
    private bool CheckAllowRush()
    {
        return !haveRushCD || _timeAfterLastRushed >= rushCD;
    }
    
    /// <summary>
    /// Checks whether the player is allowed to fly.
    /// </summary>
    /// <returns></returns>
    private bool CheckAllowFly()
    {
        return _isSpaceGravity;
    }
    
    /// <summary>
    /// Player interact with environment.
    /// </summary>
    /// <param name="isIntendInteract"></param>
    private void Interact(bool isIntendInteract)
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
    /// <summary>
    /// This is deprecated. Don't use.
    /// </summary>
    /// <returns></returns>
    bool CheckGround()
    {
        // Ray detection part.
        // Box width align to player's width. It's center at lower of player.
        Vector2 boxSize = new Vector2(Math.Abs(transform.localScale.x), groundDetectionRaycastDistance);
        Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - 1.2f);
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, groundDetectionRaycastDistance);
        return hit.collider && hit.collider.CompareTag(groundLayerMask);
    }

    private int ThreeHitGroundCheck(float upPosition = -0.5f, float maxDistance = 0.8f)
    {
        var objLength = ColliderSizeJudge(playerCollider).x;
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        
        RaycastHit2D firstHit = Physics2D.Raycast(
            transform.position + raycastUpPosition * transform.up,
            -Vector3.up, 
            raycastMaxDistance,
            LayerMask.GetMask("Ground")
            );
        if(firstHit.transform) hits.Add(firstHit);
        
        RaycastHit2D secondHit = Physics2D.Raycast(
            transform.position + raycastUpPosition * transform.up + 0.8f * objLength / 1.5f * Vector3.right, 
            -Vector3.up, 
            raycastMaxDistance, 
            LayerMask.GetMask("Ground")
            );
        if(secondHit.transform) hits.Add(secondHit);
        
        RaycastHit2D thirdHit = Physics2D.Raycast(
            transform.position + raycastUpPosition * transform.up - 0.8f * objLength / 1.5f * Vector3.right,
            -Vector3.up, 
            raycastMaxDistance, 
            LayerMask.GetMask("Ground")
            );
        if(thirdHit.transform) hits.Add(thirdHit);

        return hits.Count;

    }
    
    private static Vector3 ColliderSizeJudge(Collider2D collider2D)
    {
        return new Vector3(collider2D.bounds.size.x, collider2D.bounds.size.y, collider2D.bounds.size.z);
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
    private void Timer()
    {
        RushCountDown();
        CoyoteCountDown();
    }

    private void RushCountDown()
    {
        if(_timeAfterLastRushed < rushCD)
            _timeAfterLastRushed += Time.deltaTime;
    }

    private void CoyoteCountDown()
    {
        if (_remainingAllowCoyoteTime <= 0)
        {
            _remainingAllowCoyoteTime = 0;
            return;
        }
        
        _remainingAllowCoyoteTime -= Time.deltaTime;
    }
    #endregion
    
    #region Visualization
    // Draw the ray cast in Unity Editor.
    /// <summary>
    /// This is deprecated.
    /// </summary>
    void OnDrawGizmosNoUse()
    {
        Vector2 boxSize = new Vector2(transform.localScale.x, groundDetectionRaycastDistance);
        Vector2 boxCenter = new Vector2(transform.position.x, transform.position.y - 1.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(boxCenter, boxSize);
    }
    
    private void OnDrawGizmos()
    {
        //float upPosition = -0.5f;
        //float maxDistance = 0.8f;
        var objLength = ColliderSizeJudge(playerCollider).x;

        // First Raycast
        Gizmos.color = Color.green;
        Vector2 startPos = transform.position + raycastUpPosition * transform.up;
        Vector2 endPos = startPos + -Vector2.up * raycastMaxDistance;
        Gizmos.DrawLine(startPos, endPos);

        // Second Raycast
        Gizmos.color = Color.blue;
        startPos = transform.position + raycastUpPosition * transform.up + 0.8f * objLength / 1.5f * Vector3.right;
        endPos = startPos + -Vector2.up * raycastMaxDistance;
        Gizmos.DrawLine(startPos, endPos);

        // Third Raycast
        Gizmos.color = Color.red;
        startPos = transform.position + raycastUpPosition * transform.up - 0.8f * objLength / 1.5f * Vector3.right;
        endPos = startPos + -Vector2.up * raycastMaxDistance;
        Gizmos.DrawLine(startPos, endPos);
    }
    
    #endregion
}
