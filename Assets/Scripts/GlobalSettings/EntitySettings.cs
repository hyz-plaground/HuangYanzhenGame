public class PlayerProperties : Singleton<PlayerProperties>
{
    // Player Movement
    public float PLAYER_DEFAULT_MOVE_SPEED = 10f;
    public float PLAYER_DEFAULT_JUMP_SPEED = 10f;
    public float PLAYER_DEFAULT_RUSH_SPEED = 40f;
   
    // Player Tags
    public string PLAYER_TAG = "ENT_PLAYER";
    public string PLAYER_HAND_TAG = "ENT_PLAYER_HAND";
    
    // Detailed Player Movement
    public float PLAYER_DEFAULT_RUSH_CD = 0.3f;
    public float PLAYER_MAX_ALLOW_COYOTE_TIME = 0.8f;
    public float PLAYER_MAX_GRAVITY_SCALE = 5;
    
    // Ground Detection
    public string PLAYER_GROUND_LAYER_MASK = "Ground"; 
    public float PLAYER_GROUND_DETECTION_RAYCAST_MAX_DISTANCE = 0.8f;
    public float PLAYER_GROUND_DETECTION_RAYCAST_UP_POSITION = -0.5f;
    
    // Player Life
    public int PLAYER_DEFAULT_MAX_LIFE = 256;
    public float PLAYER_FALL_DAMAGE_THRESHOLD = 5f;
}