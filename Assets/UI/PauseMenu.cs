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
        // New Input System
        if (Keyboard.current != null)
        {
            // Quando o pauseKey for Escape
            if (pauseKey == KeyCode.Escape && Keyboard.current.escapeKey.wasPressedThisFrame)
                return true;

            // Fallback gen\u00e9rico: tenta converter KeyCode -> Key do InputSystem
            var keyString = pauseKey.ToString();
            if (System.Enum.TryParse<Key>(keyString, out var key))
                return Keyboard.current[key].wasPressedThisFrame;
        }
#endif

        // Old Input Manager (fallback)
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

        // Opcional: mostra o mouse no pause
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void OnDestroy()
    {
        // Garante que o jogo n\u00e3o fique travado ao sair/destruir o objeto.
        if (Time.timeScale == 0f)
            Time.timeScale = 1f;
    }
}
