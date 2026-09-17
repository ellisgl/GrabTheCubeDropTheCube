using UnityEngine;

public class CollisionSound : MonoBehaviour
{
    public AudioClip deskSound;
    public AudioClip floorSound;
    public AudioClip chairSound;

    [SerializeField] private float minImpactSpeed = 0.5f;
    [SerializeField] private float cooldown = 0.15f;

    private AudioSource audioSource;
    private float lastPlayTime;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude < minImpactSpeed)
            return;

        if (Time.time - lastPlayTime < cooldown)
            return;

        AudioClip clip = GetClipForTag(collision.gameObject.tag);
        if (clip == null)
            return;

        float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 5f);
        audioSource.PlayOneShot(clip, volume);
        lastPlayTime = Time.time;
    }

    AudioClip GetClipForTag(string tag)
    {
        return tag switch
        {
            "Desk" => deskSound,
            "Floor" => floorSound,
            "Chair" => chairSound,
            _ => null
        };
    }
}
