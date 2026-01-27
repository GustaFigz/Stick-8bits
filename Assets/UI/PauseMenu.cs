using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Painel do menu de pause (onde est\u00e3o os bot\u00f5es/textos).")]
    [SerializeField] private GameObject pausePanel;

    [Header("Settings")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool logToConsole = true;

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        // Em 2D/UI, nunca trave o cursor.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetPaused(false);
    }

    private void Update()
    {
        if (WasPausePressedThisFrame())
        {
            if (logToConsole) Debug.Log("[PauseMenu] Pause key pressed");
            TogglePause();
        }
    }

    private bool WasPausePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (pauseKey == KeyCode.Escape && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;

            var keyString = pauseKey.ToString();
            if (System.Enum.TryParse<Key>(keyString, out var key))
                return Keyboard.current[key].wasPressedThisFrame;
        }
#endif

        return Input.GetKeyDown(pauseKey);
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void Pause()
    {
        SetPaused(true);
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;

        if (pausePanel != null)
            pausePanel.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (logToConsole) Debug.Log($"[PauseMenu] IsPaused={IsPaused} Time.timeScale={Time.timeScale}");

        // Em pause, normalmente queremos o cursor vis\u00edvel; fora do pause tamb\u00e9m (jogo 2D).
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
