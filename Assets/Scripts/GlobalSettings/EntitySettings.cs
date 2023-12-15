public class PlayerProperties : Singleton<PlayerProperties>
{
    // Player Movement
    public float PLAYER_DEFAULT_MOVE_SPEED = 10f;
    public float PLAYER_DEFAULT_JUMP_SPEED = 10f;
    public float PLAYER_DEFAULT_RUSH_SPEED = 40f;
    public string PLAYER_GROUND_TAG = "ENV_GROUND";         // Ground Detection
    
    // Static Player Properties
    public float PLAYER_GROUND_DETECTION_RACAST_DISTANCE = 0.2f;
    public float PLAYER_DEFAULT_RUSH_CD = 1f;
    public float PLAYER_MAX_GRAVITY_SCALE = 5;
    public int PLAYER_DEFAULT_MAX_LIFE = 256;
    public float PLAYER_FALL_DAMAGE_THRESHOLD = 5f;
}