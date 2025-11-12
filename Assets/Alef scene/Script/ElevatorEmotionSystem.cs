using UnityEngine;
using UnityEngine.UI;

public class ElevatorEmotionSystem : MonoBehaviour
{
    public static ElevatorEmotionSystem Instance;

    [Header("Player Emotional Stats")]
    [Range(0, 100)] public float socialBattery = 80f;  // Batterie sociale du joueur
    [Range(0, 100)] public float ambition = 50f;       // Niveau d'ambition
    [Range(0, 100)] public float reputation = 60f;     // Réputation dans l'entreprise

    [Header("UI Bars (optional)")]
    public Slider batteryBar;     // Barre visuelle pour la batterie sociale
    public Slider ambitionBar;    // Barre visuelle pour l'ambition
    public Slider reputationBar;  // Barre visuelle pour la réputation

    [Header("Lose Conditions")]
    public bool isGameOver = false;  // Évite les doubles appels de fin

    // Événement appelé quand le joueur perd
    public delegate void OnGameOver(string reason);
    public static event OnGameOver GameOverEvent;

    void Awake()
    {
        // Système Singleton : une seule instance globale accessible
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    // --------------------------------------------------------------
    // 🧩 Modification d'un paramètre émotionnel
    // --------------------------------------------------------------
    public void Modify(string type, float amount)
    {
        if (isGameOver) return;

        switch (type)
        {
            case "battery":
                socialBattery = Mathf.Clamp(socialBattery + amount, 0, 100);
                break;
            case "ambition":
                ambition = Mathf.Clamp(ambition + amount, 0, 100);
                break;
            case "reputation":
                reputation = Mathf.Clamp(reputation + amount, 0, 100);
                break;
        }

        UpdateUI();
        CheckLoseConditions();
    }

    // --------------------------------------------------------------
    // 🧠 Vérifie si le joueur a les statistiques nécessaires
    // --------------------------------------------------------------
    public bool HasStat(string stat, float minValue)
    {
        return stat switch
        {
            "battery" => socialBattery >= minValue,
            "ambition" => ambition >= minValue,
            "reputation" => reputation >= minValue,
            _ => false
        };
    }

    // --------------------------------------------------------------
    // 💀 Vérifie les conditions de défaite
    // --------------------------------------------------------------
    void CheckLoseConditions()
    {
        if (isGameOver) return;

        if (socialBattery <= 0)
        {
            TriggerGameOver("You burned out. Your social battery is empty.");
        }
        else if (reputation <= 0)
        {
            TriggerGameOver("You lost your reputation. You got fired.");
        }
    }

    // --------------------------------------------------------------
    // 🚨 Lancer un Game Over
    // --------------------------------------------------------------
    void TriggerGameOver(string reason)
    {
        isGameOver = true;
        Debug.Log("Game Over: " + reason);

        // Envoie l'événement aux autres systèmes (ElevatorGameManager)
        GameOverEvent?.Invoke(reason);
    }

    // --------------------------------------------------------------
    // 🎚️ Mise à jour des barres de progression UI
    // --------------------------------------------------------------
    void UpdateUI()
    {
        if (batteryBar != null)
            batteryBar.value = socialBattery / 100f;

        if (ambitionBar != null)
            ambitionBar.value = ambition / 100f;

        if (reputationBar != null)
            reputationBar.value = reputation / 100f;
    }

    // --------------------------------------------------------------
    // 🩺 Debug rapide dans l'éditeur
    // --------------------------------------------------------------
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), "Battery: " + socialBattery.ToString("F0"));
        GUI.Label(new Rect(10, 30, 300, 20), "Ambition: " + ambition.ToString("F0"));
        GUI.Label(new Rect(10, 50, 300, 20), "Reputation: " + reputation.ToString("F0"));
    }
}
