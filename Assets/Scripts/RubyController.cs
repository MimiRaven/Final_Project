using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RubyController : MonoBehaviour
{
    public float speed = 3.0f;

    public int maxHealth = 5;
    public int currentScene;
    public float timeInvincible = 2.0f;

    public AudioClip throwSound;
    public AudioClip hitSound;
    public AudioClip backgroundMusic;
    public AudioClip winMusic;
    public AudioClip loseMusic;
    public AudioClip jambiSound;

    public GameObject projectilePrefab;
    public GameObject healthincreasePrefab;
    public GameObject hiteffectPrefab;

    public int score = 0;

    public Text scoreText;
    public Text winText;
    public Text lostText;
    public Text cogText;

    private float boostTimer;
    private bool boosting;

    public int health { get { return currentHealth; }}
    int currentHealth;
    int currentCog;

    bool gameOver;
    bool isInvincible;
    float invincibleTimer;

    Rigidbody2D rigidbody2d;
    float horizontal;
    float vertical;

    Animator animator;
    AudioSource audioSource;

    Vector2 lookDirection = new Vector2(1,0);

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        currentCog = 4;
        boostTimer = 0;
        boosting = false;
        cogText.text = "Cogs: " + currentCog;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        scoreText.text = "Fixed Robots: " + score.ToString();
        winText.text = "";
        lostText.text = "";
        audioSource.clip = backgroundMusic;
        audioSource.Play();
        audioSource.loop = true;
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontal, vertical);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            lookDirection.Set(move.x, move.y);
            lookDirection.Normalize();
        }

        animator.SetFloat("Look X", lookDirection.x);
        animator.SetFloat("Look Y", lookDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer < 0)
                isInvincible = false;
        }

        if (Input.GetKeyDown(KeyCode.C) && currentCog != 0)
        {
            Launch();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            RaycastHit2D hit = Physics2D.Raycast(rigidbody2d.position + Vector2.up * 0.2f, lookDirection, 1.5f, LayerMask.GetMask("NPC"));
            if (hit.collider != null)
            {
                audioSource.PlayOneShot(jambiSound);

                if (score == 4)
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        SceneManager.LoadScene("SecondScene");
                    }
                }

                else
                {
                    NonPlayerCharacter character = hit.collider.GetComponent<NonPlayerCharacter>();
                    if (character != null)
                    {
                        character.DisplayDialog();
                    }
                }
            }
        }

        if (Input.GetKey(KeyCode.R))

        {
            if (gameOver == true)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // this loads the currently active scene
            }
        }

        if (Input.GetKey("escape"))
        {
            Application.Quit();
        }

        if(boosting)
        {
            boostTimer += Time.deltaTime;
            if(boostTimer >= 2)
            {
                speed = 3;
                boostTimer = 0;
                boosting = false;
            }
        }

        if (currentHealth <= 0)
        {
            lostText.text = "You Lost! Press R to Restart";
            gameOver = true;
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            speed = 0;
        }

        if(score == 4)
        {
            currentScene = SceneManager.GetActiveScene().buildIndex;
            if(currentScene == 0)
            {
                winText.text = "Talk to Jambi to visit Stage Two";
            }
        }

        if (score == 5)
        {
            winText.text = "You Win! Press R to Restart. Game Created By: Mia Torres";
            if (Input.GetKey(KeyCode.R))
            {
                SceneManager.LoadScene("FirstScene");
            }
        }
    }

    void FixedUpdate()
    {
        Vector2 position = rigidbody2d.position;
        position.x = position.x + speed * horizontal * Time.deltaTime;
        position.y = position.y + speed * vertical * Time.deltaTime;

        rigidbody2d.MovePosition(position);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Crystal")
        {
            boosting = true;
            speed = 5;
            Destroy(other.gameObject);
        }
    }

    public void ChangeHealth(int amount)
    {
        if (amount < 0)
        {
            animator.SetTrigger("Hit");
            if (isInvincible)
                return;

            isInvincible = true;
            invincibleTimer = timeInvincible;
            PlaySound(hitSound);
            GameObject hiteffectObject = Instantiate(hiteffectPrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        if (amount > 0)
        {
            GameObject healthincreaseObject = Instantiate(healthincreasePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UIHealthBar.instance.SetValue(currentHealth / (float)maxHealth);

        if(currentHealth == 0)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Stop();
            audioSource.clip = loseMusic;
            audioSource.Play();
            audioSource.loop = false;
        }
    }

    public void ChangeScore(int scoreAmount)
    {
        score += 1;
        scoreText.text = "Fixed Robots: " + score.ToString();
        
        if(score == 5)
        {
            audioSource.clip = backgroundMusic;
            audioSource.Stop();
            audioSource.clip = winMusic;
            audioSource.Play();
            audioSource.loop = false;
        }
        
    }

    public void ChangeCog(int amount)
    {
        currentCog = currentCog + amount;
        cogText.text = "Cogs: " + currentCog;
    }

    void Launch()
    {
        GameObject projectileObject = Instantiate(projectilePrefab, rigidbody2d.position + Vector2.up * 0.5f, Quaternion.identity);

        Projectile projectile = projectileObject.GetComponent<Projectile>();
        projectile.Launch(lookDirection, 300);

        animator.SetTrigger("Launch");
        currentCog = currentCog - 1;
        cogText.text = "Cogs: " + currentCog;
        PlaySound(throwSound);
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
