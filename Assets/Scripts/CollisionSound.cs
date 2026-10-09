using UnityEngine;
using Oculus.Interaction;

public class CollisionSound : MonoBehaviour
{
    // Create public arrays for different collision sounds
    public AudioClip[] deskSounds;
    public AudioClip[] floorSounds;
    public AudioClip[] chairSounds;
    public AudioClip[] grabSounds;

    // Minimum impact speed to trigger sound
    [SerializeField] private float minImpactSpeed = 0.5f;
    // Cooldown time between sound plays to prevent overlapping sounds
    [SerializeField] private float cooldown = 0.15f;

    /// Private variables to manage audio playback
    private AudioSource audioSource;
    private Grabbable grabbable;
    /// Track the last time a sound was played
    private float lastPlayTime;

    void Awake()
    {
        // Initialize the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        grabbable = GetComponent<Grabbable>();
    }

    void OnEnable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised += HandlePointerEvent;
        }
    }

    void OnDisable()
    {
        if (grabbable != null)
        {
            grabbable.WhenPointerEventRaised -= HandlePointerEvent;
        }
    }

    void HandlePointerEvent(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type != PointerEventType.Select || grabSounds == null || grabSounds.Length == 0)
        {
            return;
        }

        AudioClip clip = grabSounds[Random.Range(0, grabSounds.Length)];
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }


    /// <summary>
    /// This method is called when a collision occurs.
    /// It checks the collision's relative velocity and plays a sound if the conditions are met.
    /// </summary>
    /// <param name="collision">The Collision object containing information about the collision.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision's relative velocity is below the minimum impact speed
        if (collision.relativeVelocity.magnitude < minImpactSpeed) {
            return;
        }
        
        // Check if enough time has passed since the last sound was played
        // This prevents overlapping sounds and ensures a cooldown period between sound plays
        if (Time.time - lastPlayTime < cooldown) {
            return;
        }

        // Get the appropriate audio clips based on the tag of the collided object
        AudioClip[] clips = GetClipsForTag(collision.gameObject.tag);
        if (clips == null || clips.Length == 0) {
            return;
        }

        // Get the audio clip to play
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) {
            return;
        }

        /// Calculate the volume based on the collision's relative velocity, clamped between 0 and 1
        float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 5f);
        audioSource.PlayOneShot(clip, volume);
        lastPlayTime = Time.time;
    }

    /// <summary>
    /// This method returns an array of AudioClips based on the tag of the collided object.
    /// </summary>
    /// <param name="tag">The tag of the collided object.</param>    
    AudioClip[] GetClipsForTag(string tag)
    {
        return tag switch
        {
            "DeskTop" => deskSounds,
            "Floor" => floorSounds,
            "Chair" => chairSounds,
            _ => null
        };
    }
}
