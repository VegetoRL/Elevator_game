using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorGameManager : MonoBehaviour
{
    [Header("Door Control")]
    public Animator leftDoorAnimator;    // Animation de la porte gauche
    public Animator rightDoorAnimator;   // Animation de la porte droite
    public float doorOpenTime = 1.5f;    // Durée d'ouverture
    public float doorCloseTime = 1.5f;   // Durée de fermeture

    [Header("3D Backgrounds")]
    public List<GameObject> elevatorBackgrounds; // Décors 3D visibles par les portes
    private int currentBackground = 0;

    [Header("NPC Management")]
    public List<GameObject> npcList; // Liste de tous les collègues
    public Transform npcSpawnPoint;  // Position d'apparition
    private int currentNPC = 0;

    [Header("Dialogue System")]
    public ElevatorDialogueManager dialogueManager;
    public GameObject talkPanel; // Panneau "Do you want to talk?"

    private bool isTransitioning = false;

    void Start()
    {
        // Cache tous les décors sauf le premier
        for (int i = 0; i < elevatorBackgrounds.Count; i++)
            elevatorBackgrounds[i].SetActive(i == 0);

        // Cache tous les NPC
        foreach (var npc in npcList)
            npc.SetActive(false);

        // Démarre la première ouverture de porte
        StartCoroutine(OpenDoorsSequence());
    }

    // ---------------------- SEQUENCE D'OUVERTURE ----------------------

    IEnumerator OpenDoorsSequence()
    {
        isTransitioning = true;

        // Ouvre les portes
        leftDoorAnimator.SetTrigger("Open");
        rightDoorAnimator.SetTrigger("Open");

        yield return new WaitForSeconds(doorOpenTime);

        // Active le prochain NPC
        SpawnNextNPC();

        isTransitioning = false;
    }

    void SpawnNextNPC()
    {
        if (currentNPC >= npcList.Count)
        {
            StartCoroutine(EndGameSequence());
            return;
        }

        // Désactive tous les NPC, active seulement le suivant
        for (int i = 0; i < npcList.Count; i++)
            npcList[i].SetActive(i == currentNPC);

        currentNPC++;

        // Le NPC est maintenant visible, le joueur peut choisir de parler
        talkPanel.SetActive(true);
    }

    // ---------------------- SEQUENCE DE FERMETURE ----------------------

    public void EndConversation()
    {
        if (!isTransitioning)
            StartCoroutine(CloseDoorsAndChangeFloor());
    }

    IEnumerator CloseDoorsAndChangeFloor()
    {
        isTransitioning = true;

        // Ferme les portes
        leftDoorAnimator.SetTrigger("Close");
        rightDoorAnimator.SetTrigger("Close");

        yield return new WaitForSeconds(doorCloseTime);

        // Change le fond (simulateur d'étage)
        elevatorBackgrounds[currentBackground].SetActive(false);
        currentBackground = (currentBackground + 1) % elevatorBackgrounds.Count;
        elevatorBackgrounds[currentBackground].SetActive(true);

        yield return new WaitForSeconds(0.5f);

        // Ouvre à nouveau pour le prochain collègue
        StartCoroutine(OpenDoorsSequence());
    }

    // ---------------------- FIN DU JEU ----------------------

    IEnumerator EndGameSequence()
    {
        leftDoorAnimator.SetTrigger("Close");
        rightDoorAnimator.SetTrigger("Close");
        yield return new WaitForSeconds(doorCloseTime);
        Debug.Log("End of day — all conversations finished.");
        SceneManager.LoadScene("EndScene");
    }

    // ---------------------- GAME OVER ----------------------

    void OnEnable()
    {
        ElevatorEmotionSystem.GameOverEvent += OnGameOver;
    }

    void OnDisable()
    {
        ElevatorEmotionSystem.GameOverEvent -= OnGameOver;
    }

    void OnGameOver(string reason)
    {
        StartCoroutine(GameOverSequence(reason));
    }

    IEnumerator GameOverSequence(string reason)
    {
        Debug.Log("Game Over: " + reason);
        leftDoorAnimator.SetTrigger("Close");
        rightDoorAnimator.SetTrigger("Close");

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene("GameOverScene");
    }
}
