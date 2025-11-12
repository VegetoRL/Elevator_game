using UnityEngine;

public class ElevatorIdleAnimation : MonoBehaviour
{
    public Sprite[] idleSprites; // Images d'animation (sprites)
    public float frameRate = 0.2f; // Durée entre chaque image
    public bool useAnimation = true;

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("No SpriteRenderer found on " + gameObject.name);
            enabled = false;
        }
    }

    void Update()
    {
        if (!useAnimation || idleSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            currentFrame = (currentFrame + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[currentFrame];
            timer = 0f;
        }
    }
}