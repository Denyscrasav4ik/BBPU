using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ukrainization.API
{
    public class MenuTextureManager : MonoBehaviour
    {
        public static MenuTextureManager Instance { get; private set; } = null!;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu")
            {
                API.Logger.Info("Головне меню завантажено, повторне використання текстур...");
                TPPlugin.Instance.ApplyMenuTextures();
            }
        }
    }
}
