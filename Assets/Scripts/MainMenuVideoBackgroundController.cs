using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MainMenuVideoBackgroundController : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;
    public VideoClip videoClip;
    public RenderTexture renderTexture;

    float nextLogTime;

    void Awake()
    {
        EnsureReady("Awake");
    }

    void OnEnable()
    {
        EnsureReady("OnEnable");
#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
        EditorApplication.update += EditorTick;
#endif
    }

    void Start()
    {
        EnsureReady("Start");
    }

    void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnPrepared;
            videoPlayer.errorReceived -= OnVideoError;
        }

#if UNITY_EDITOR
        EditorApplication.update -= EditorTick;
#endif
    }

    void Update()
    {
        EnsureReady("Update");
        BindVisibleTexture();
        KeepPlaying();
        LogStatus("Update");
    }

#if UNITY_EDITOR
    void EditorTick()
    {
        if (this == null || Application.isPlaying) return;
        EnsureReady("EditorTick");
        BindVisibleTexture();
        KeepPlaying();
        LogStatus("EditorTick");
        EditorApplication.QueuePlayerLoopUpdate();
    }
#endif

    void EnsureReady(string source)
    {
        ResolveReferences();
        ConfigureRect();
        ConfigureRawImage();
        ConfigureVideoPlayer();
        PrepareOrPlay(source);
    }

    void ResolveReferences()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        if (videoPlayer == null)
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
    }

    void ConfigureRect()
    {
        RectTransform rect = transform as RectTransform;
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        CanvasGroup group = GetComponent<CanvasGroup>();
        if (group == null)
            group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    void ConfigureRawImage()
    {
        if (rawImage == null)
            return;

        rawImage.enabled = true;
        rawImage.color = Color.white;
        rawImage.raycastTarget = false;

        if (!Application.isPlaying && renderTexture != null)
            rawImage.texture = renderTexture;
        else if (rawImage.texture == null && renderTexture != null)
            rawImage.texture = renderTexture;
    }

    void ConfigureVideoPlayer()
    {
        if (videoPlayer == null)
            return;

        videoPlayer.gameObject.SetActive(true);
        videoPlayer.enabled = true;
        videoPlayer.source = VideoSource.VideoClip;
        if (videoClip != null)
            videoPlayer.clip = videoClip;

        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        videoPlayer.waitForFirstFrame = false;
        videoPlayer.skipOnDrop = true;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived -= OnVideoError;
        videoPlayer.errorReceived += OnVideoError;
    }

    void PrepareOrPlay(string source)
    {
        if (videoPlayer == null || videoPlayer.clip == null)
            return;

        if (!videoPlayer.isPrepared)
            videoPlayer.Prepare();

        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
            videoPlayer.Play();

        LogStatus(source);
    }

    void OnPrepared(VideoPlayer player)
    {
        if (player == null)
            return;

        player.Play();
        BindVisibleTexture();
        LogStatus("Prepared");
    }

    void KeepPlaying()
    {
        if (videoPlayer == null || videoPlayer.clip == null)
            return;

        if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
            videoPlayer.Play();
    }

    void BindVisibleTexture()
    {
        if (rawImage == null || videoPlayer == null)
            return;

        Texture texture = videoPlayer.texture;
        if (texture != null)
            rawImage.texture = texture;
        else if (rawImage.texture == null && renderTexture != null)
            rawImage.texture = renderTexture;
    }

    void OnVideoError(VideoPlayer player, string message)
    {
        Debug.LogError($"MainMenuVideoBackground video error: {message}", this);
    }

    void LogStatus(string source)
    {
        if (Time.realtimeSinceStartup < nextLogTime)
            return;

        nextLogTime = Time.realtimeSinceStartup + 1f;
        string clipName = videoPlayer != null && videoPlayer.clip != null ? videoPlayer.clip.name : "<none>";
        string imageTexture = rawImage != null && rawImage.texture != null ? rawImage.texture.name : "<none>";
        string playerTexture = videoPlayer != null && videoPlayer.texture != null ? videoPlayer.texture.name : "<none>";
        bool prepared = videoPlayer != null && videoPlayer.isPrepared;
        bool playing = videoPlayer != null && videoPlayer.isPlaying;
        long frame = videoPlayer != null ? videoPlayer.frame : -1;
        double time = videoPlayer != null ? videoPlayer.time : -1d;
        Debug.Log($"MainMenuVideoBackground[{source}] clip={clipName} prepared={prepared} playing={playing} frame={frame} time={time:F2} rawTexture={imageTexture} playerTexture={playerTexture}", this);
    }
}
