using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class ElevatorDialogueEntry
{
    public string speakerName;
    public string lineText;
    public bool isPlayerSpeaking;
    public bool isChoice;
    public List<ElevatorDialogueChoice> choices = new();
}

[System.Serializable]
public class ElevatorDialogueChoice
{
    public string text;
    public int nextIndex;
    public bool isNeutral;
    public Dictionary<string, float> effects = new(); // effets sur battery / ambition / reputation
    public string requiredStat;
    public float requiredValue;
}

public class ElevatorDialogueManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject dialoguePanel;
    public Image dialogueBoxImage;
    public Image npcImage;
    public Image playerImage;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI playerNameText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;

    [Header("Sprites")]
    public Sprite npcSprite;
    public Sprite playerSprite;
    public Sprite npcTextboxSprite;
    public Sprite playerTextboxSprite;

    private List<ElevatorDialogueEntry> dialogueLines = new();
    private int currentLine = 0;
    private bool dialogueActive = false;

    void Start()
    {
        // Le panneau de dialogue est caché au départ
        dialoguePanel.SetActive(false);
    }

    // Charge un fichier de dialogue et prépare les données
    public void LoadDialogueFromFile(string filename)
    {
        dialogueLines.Clear();
        string path = Path.Combine(Application.streamingAssetsPath, filename);

        if (!File.Exists(path))
        {
            Debug.LogError("Dialogue file not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        ElevatorDialogueEntry current = null;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("[CHOICE]"))
            {
                if (current != null)
                    current.isChoice = true;
                continue;
            }

            // Lecture d'une ligne simple (ex: NPC: Bonjour)
            if (line.Contains(":") && !line.Contains("->"))
            {
                int sep = line.IndexOf(":");
                string speaker = line[..sep].Trim();
                string text = line[(sep + 1)..].Trim();

                current = new ElevatorDialogueEntry
                {
                    speakerName = speaker,
                    lineText = text,
                    isPlayerSpeaking = speaker.ToLower().Contains("player")
                };

                dialogueLines.Add(current);
            }
            // Lecture des choix du joueur
            else if (line.Contains("->") && current != null)
            {
                string[] parts = line.Split("->");
                string choiceText = parts[0].Trim();
                int next = int.Parse(parts[1].Split('(')[0].Trim());

                var choice = new ElevatorDialogueChoice
                {
                    text = choiceText,
                    nextIndex = next
                };

                // Option neutre
                if (line.Contains("(neutral)"))
                    choice.isNeutral = true;

                // Effets (battery, ambition, reputation)
                string[] stats = { "battery", "ambition", "reputation" };
                foreach (string stat in stats)
                {
                    if (line.Contains($"({stat}"))
                    {
                        int start = line.IndexOf($"({stat}") + stat.Length + 1;
                        int end = line.IndexOf(")", start);
                        string valStr = line.Substring(start, end - start).Trim();
                        if (float.TryParse(valStr, out float val))
                            choice.effects[stat] = val;
                    }
                }

                // Conditions spéciales (requires ambition 70)
                if (line.Contains("(requires"))
                {
                    int start = line.IndexOf("(requires") + 9;
                    int end = line.IndexOf(")", start);
                    string[] req = line.Substring(start, end - start).Trim().Split(' ');
                    if (req.Length == 2)
                    {
                        choice.requiredStat = req[0];
                        float.TryParse(req[1], out choice.requiredValue);
                    }
                }

                current.choices.Add(choice);
            }
        }
    }

    // Lance le dialogue
    public void StartDialogue()
    {
        currentLine = 0;
        dialoguePanel.SetActive(true);
        dialogueActive = true;
        ShowLine();
    }

    // Affiche la ligne actuelle
    void ShowLine()
    {
        if (currentLine < 0 || currentLine >= dialogueLines.Count)
        {
            EndDialogue();
            return;
        }

        var line = dialogueLines[currentLine];
        dialogueText.text = line.lineText;

        // Le joueur est toujours à gauche
        playerNameText.text = line.isPlayerSpeaking ? line.speakerName : "";
        npcNameText.text = line.isPlayerSpeaking ? "" : line.speakerName;

        // Définit les sprites et la transparence
        dialogueBoxImage.sprite = line.isPlayerSpeaking ? playerTextboxSprite : npcTextboxSprite;
        playerImage.sprite = playerSprite;
        npcImage.sprite = npcSprite;

        playerImage.color = line.isPlayerSpeaking ? Color.white : new Color(1f, 1f, 1f, 0.4f);
        npcImage.color = line.isPlayerSpeaking ? new Color(1f, 1f, 1f, 0.4f) : Color.white;

        playerNameText.alignment = TextAlignmentOptions.Left;
        npcNameText.alignment = TextAlignmentOptions.Right;

        // Supprime les anciens boutons
        foreach (Transform c in choiceContainer)
            Destroy(c.gameObject);

        // Affiche les choix du joueur
        if (line.isChoice)
        {
            foreach (var choice in line.choices)
            {
                bool canShow = true;

                if (!string.IsNullOrEmpty(choice.requiredStat))
                    canShow = ElevatorEmotionSystem.Instance.HasStat(choice.requiredStat, choice.requiredValue);

                if (!canShow) continue;

                var btn = Instantiate(choiceButtonPrefab, choiceContainer);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = choice.text;
                int next = choice.nextIndex;

                btn.onClick.AddListener(() =>
                {
                    ApplyEffects(choice);
                    currentLine = next;
                    ShowLine();
                });
            }
        }
    }

    // Applique les effets de la réponse choisie
    void ApplyEffects(ElevatorDialogueChoice choice)
    {
        if (choice.isNeutral) return;
        foreach (var e in choice.effects)
            ElevatorEmotionSystem.Instance.Modify(e.Key, e.Value);
    }

    void Update()
    {
        if (!dialogueActive) return;

        if (Input.GetMouseButtonDown(1))
        {
            currentLine++;
            ShowLine();
        }
    }

    // Termine le dialogue
    public void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueActive = false;
    }
}
