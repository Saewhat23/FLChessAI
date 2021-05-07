using UnityEngine;

/**
 * SoundManager
 *
 * Manages the playback of sound effects when certain actions are performed.
 */
[RequireComponent(typeof(AudioSource))]
public class SoundManger : MonoBehaviour
{
    private static AudioClip _pMoveSound;
    private static AudioClip _pCaptureSound;
    private static AudioClip _pAttackFailSound;
    private static AudioClip _diceRollSound;
    private static AudioClip _attackSuccess;
    private static AudioSource _audioSrc;

    void Start()
    {
        _pMoveSound = Resources.Load<AudioClip>("MoveSound");                   // Movement sound
        _pCaptureSound = Resources.Load<AudioClip>("CaptureSound");             // Capture sound
        _pAttackFailSound = Resources.Load<AudioClip>("AttackFailSound");       // Attack/fail sound
        _diceRollSound = Resources.Load<AudioClip>("DiceRollSound");            // Die roll sound
        _attackSuccess = Resources.Load<AudioClip>("AttackSuccessSound");       // Attack success sound
        _audioSrc =  GetComponent<AudioSource>();
    }

    public static void PlayMoveSound()
    {
        _audioSrc.volume = 0.7f;
        _audioSrc.PlayOneShot(_pMoveSound);
    }

    public static void PlayCaptureSound()
    {
        _audioSrc.volume = 0.3f;
        _audioSrc.PlayOneShot(_pCaptureSound);
    }

    public static void PlayAtackFailedSound()
    {
        _audioSrc.volume = 0.6f;
        _audioSrc.PlayOneShot(_pAttackFailSound);
       
        
    }
    public static void PlayDiceRollSound()
    {
        _audioSrc.volume = 0.9f;
        _audioSrc.PlayOneShot(_diceRollSound);
    }

    public static void PlayAttackSuccessSound()
    {
        _audioSrc.volume = 0.5f;
        _audioSrc.PlayOneShot(_attackSuccess);
    }
}
