using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 3; // Speed of the player movement
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private Rigidbody2D rb20;

    private float move;

    public float jumpForce = 4; // Force applied when the player jumps
    private bool isGrounded; // Flag to check if the player is on the ground

    public Transform groundCheck; // Position to check if the player is grounded
    public float groundRadius = 0.1f; // Radius for the ground check
    public LayerMask groundLayer; // Layer mask to identify what is considered ground

    private Animator animator; // Reference to the Animator component


    private int coins = 0;
    public TMP_Text textConins; // Reference to the TextMeshPro text component to display the coin count


    void Start()
    {
        rb20 = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Get the Animator component attached to the player
    }

    // Update is called once per frame
    void Update()
    {
        move = Input.GetAxis("Horizontal"); // Get horizontal input (A/D or Left/Right arrow keys)
        rb20.linearVelocity = new Vector2(move * speed, rb20.linearVelocity.y); // Move the player horizontally while keeping the vertical velocity unchanged
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1); // Flip the player sprite based on the direction of movement
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb20.linearVelocity = new Vector2(rb20.linearVelocity.x, jumpForce); // Apply a vertical force to make the player jump
        }

        animator.SetFloat("Speed", Mathf.Abs(move)); // Set the "Speed" parameter in the Animator to control animations based on movement speed
        animator.SetFloat("VerticalVelocity", rb20.linearVelocity.y); // Set the "VerticalVelocity" parameter in the Animator to control animations based on vertical movement (e.g., jumping or falling)
        animator.SetBool("IsGrounded", isGrounded); // Set the "IsGrounded" parameter in the Animator to control animations based on whether the player is on the ground or in the air


    }
    private void FixedUpdate()
    {
        // Check if the player is grounded by checking for collisions with the ground layer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Coin"))
        {
            Destroy(collision.gameObject); // Destroy the coin when the player collides with it
            coins++; // Increment the coin count
            textConins.text = coins.ToString(); // Update the coin count display

        }

        if (collision.transform.CompareTag("Spikes"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene when the player collides with spikes
        }



    }


}
