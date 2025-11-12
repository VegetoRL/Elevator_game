using UnityEngine;

// S'assure que ce script est bien sur un objet qui a une Caméra
[RequireComponent(typeof(Camera))]
public class LetterboxController : MonoBehaviour
{
    // Le ratio d'aspect que vous visez (ex: 16:9)
    public Vector2 targetAspectRatio = new Vector2(16, 9);

    // Référence à notre caméra
    private Camera cam;

    // Pour ne pas recalculer à chaque frame, seulement si la résolution change
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        // Récupère la caméra attachée à cet objet
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Pas de caméra trouvée pour le LetterboxController !");
            return;
        }

        // S'assure que la caméra ne dessine pas sur tout l'écran par défaut
        cam.rect = new Rect(0, 0, 1, 1);
    }

    void Start()
    {
        // Lance le calcul une première fois
        UpdateViewportRect();
    }

    void Update()
    {
        // Vérifie si la résolution de l'écran a changé
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            // Si oui, recalcule le rectangle de la caméra
            UpdateViewportRect();
        }
    }

    void UpdateViewportRect()
    {
        // Calcule le ratio cible (ex: 16 / 9 = 1.777)
        float targetAspect = targetAspectRatio.x / targetAspectRatio.y;

        // Calcule le ratio actuel de l'écran
        float screenAspect = (float)Screen.width / Screen.height;

        // Calcule le facteur d'échelle (combien on doit "rétrécir" la vue)
        float scaleHeight = screenAspect / targetAspect;

        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1.0f) // Écran plus "haut" que le ratio cible (ex: 4:3) -> Letterboxing (barres H/B)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f; // Centre verticalement
        }
        else // Écran plus "large" que le ratio cible (ex: 21:9) -> Pillarboxing (barres G/D)
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f; // Centre horizontalement
            rect.y = 0;
        }

        // Applique le nouveau rectangle à la caméra
        cam.rect = rect;

        // Met à jour les valeurs de vérification
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}