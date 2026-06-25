using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Источники")]
    public AudioSource musicSource; // secret / play – фоновая музыка
    public AudioSource sfxSource;   // короткие звуки

    [Header("Клипы")]
    public AudioClip secretClip;  // музыка меню / после игры
    public AudioClip playClip;    // НОВОЕ: музыка во время игры
    public AudioClip winClip;     // победа
    public AudioClip failClip;    // проигрыш
    public AudioClip boomClip;    // взрыв

    public AudioClip hotClip;     // рычаг в красное положение
    public AudioClip coldClip;    // рычаг в синее положение

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------- МЕНЮ / SECRET ----------

    public void PlayMenuMusic()
    {
        if (musicSource == null || secretClip == null) return;

        musicSource.Stop();
        musicSource.clip = secretClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMenuMusic()
    {
        if (musicSource == null) return;
        if (musicSource.clip == secretClip)
            musicSource.Stop();
    }

    // ---------- ИГРОВАЯ МУЗЫКА (play) ----------

    public void PlayGameMusic()
    {
        if (musicSource == null || playClip == null) return;

        musicSource.Stop();
        musicSource.clip = playClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopGameMusic()
    {
        if (musicSource == null) return;
        if (musicSource.clip == playClip)
            musicSource.Stop();
    }

    // ---------- ОТДЕЛЬНЫЕ ЗВУКИ ----------

    public void PlayWin()
    {
        if (sfxSource == null || winClip == null) return;
        sfxSource.PlayOneShot(winClip);
    }

        // рычаг -> горячо
    public void PlayHot()
    {
        if (sfxSource == null || hotClip == null) return;
        sfxSource.PlayOneShot(hotClip);
    }

    // рычаг -> холодно
    public void PlayCold()
    {
        if (sfxSource == null || coldClip == null) return;
        sfxSource.PlayOneShot(coldClip);
    }

    // win -> потом secret
    public void PlayWinThenSecret()
    {
        if (sfxSource == null || winClip == null) return;
        StartCoroutine(WinThenSecretRoutine());
    }

    IEnumerator WinThenSecretRoutine()
    {
        // останавливаем игровую музыку
        StopGameMusic();

        sfxSource.Stop();
        sfxSource.clip = winClip;
        sfxSource.loop = false;
        sfxSource.Play();

        yield return new WaitForSeconds(winClip.length + 0.1f);

        // возвращаем музыку меню (secret)
        PlayMenuMusic();
    }

    // boom -> fail -> secret
    public void PlayBoomFailThenSecret()
    {
        if (sfxSource == null) return;
        StartCoroutine(BoomFailThenSecretRoutine());
    }

    IEnumerator BoomFailThenSecretRoutine()
    {
        // останавливаем игровую музыку
        StopGameMusic();

        if (boomClip != null)
        {
            sfxSource.Stop();
            sfxSource.clip = boomClip;
            sfxSource.loop = false;
            sfxSource.Play();
            yield return new WaitForSeconds(boomClip.length + 0.1f);
        }

        if (failClip != null)
        {
            sfxSource.clip = failClip;
            sfxSource.loop = false;
            sfxSource.Play();
            yield return new WaitForSeconds(failClip.length + 0.1f);
        }

        // после неудачи снова secret
        PlayMenuMusic();
    }
}
