using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController2D : MonoBehaviour
{
    public PlayerData playerData;
    //movement
    // Start is called before the first frame update
    private float jumpTimeCounter; // Counter for tracking how long the jump button is held
    bool isJumping = false;
    bool facingRight = true;
    public Animator animator; //Drag in the animator here
    private Rigidbody2D rb2d; // Reference to the Rigidbody2D component
    public AudioSource jumpSound;

    //switch Weapon
    int CurrentWeaponNo; //this is what you need to change weapons
    bool isGunActive = true;
    bool isKnifeActive = false;

    //weapon & aiming
    public Transform Gun;
    Vector2 direction;
    public GameObject Bullet;
    public float BulletSpeed;
    public Transform ShootPoint;
    private Animator aimingAnimation;
    public AudioSource gunSoundEffect;

    //Ammo
    private int currentAmmo;
    private bool isReloading = false;
    private UIManager uiManagerReload;
    private Animator reloadAnimation;
    private Animator runningReloadAnimation;
    public AudioSource gunReloadSoundEffect;

    //melee Attack
    private Animator meleeAttackAnimation;
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayer;
    public float attackRate = 2f;
    float nextAttackTime = 0f;
    private bool canDealDamage = false;
    private AudioSource meleeSoundEffect;



    //fire rate
    float nextFire;

    //isGrounded
    private BoxCollider2D coll;
    [SerializeField] private LayerMask jumpableGround;

    //damage enemmy
    public int damage;
    public float delayBeforeDamage = 0.2f;


    // Start is called before the first frame update
    void Start()
    {
        // Get the AudioSource component attached to the same GameObject
        meleeSoundEffect = GetComponent<AudioSource>();

        // Get the Rigidbody2D component
        rb2d = GetComponent<Rigidbody2D>();

        // Set the initial velocity to zero
        rb2d.velocity = Vector2.zero;

        nextFire = 0f; 
        coll = GetComponent<BoxCollider2D>();

        currentAmmo = playerData.maxAmmo;

        uiManagerReload = GameObject.Find("Canvas").GetComponent<UIManager>();

        reloadAnimation = GetComponentInParent<Animator>();

        meleeAttackAnimation = GetComponentInParent<Animator>();

        aimingAnimation = GetComponentInParent<Animator>();

        runningReloadAnimation = GetComponentInParent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Horizontal Movement
        float moveX = Input.GetAxis("Horizontal");
        float targetVelocityX = moveX * playerData.speed; // Calculate the target velocity based on input and maximum speed

        // Check if the player is alive before processing movement inputs
        if (GetComponent<PlayerHealth>().IsAlive())
        {
            // Check if grounded or in the air
            if (isGrounded())
            {
                // Grounded movement with acceleration
                targetVelocityX = moveX * playerData.speed;

                // Gradually adjust velocity towards the target velocity
                rb2d.velocity = new Vector2(Mathf.MoveTowards(rb2d.velocity.x, targetVelocityX, playerData.acceleration * Time.deltaTime), rb2d.velocity.y);

                // If the sign of the target velocity is different from the current velocity, directly set the velocity to the target
                if (Mathf.Sign(targetVelocityX) != Mathf.Sign(rb2d.velocity.x))
                {
                    rb2d.velocity = new Vector2(targetVelocityX, rb2d.velocity.y);
                }
                else
                {
                    // Gradually increase velocity towards the target velocity
                    rb2d.velocity = new Vector2(Mathf.MoveTowards(rb2d.velocity.x, targetVelocityX, playerData.acceleration * Time.deltaTime), rb2d.velocity.y);
                }
            }
            else
            {
                // Air movement without acceleration
                // Directly set the horizontal velocity based on input
                rb2d.velocity = new Vector2(moveX * playerData.airMovementSpeed, rb2d.velocity.y);
            }

            //If character is moving play the run animation
            animator.SetFloat("Speed", Mathf.Abs(rb2d.velocity.x));

            // Jumping
            if (Input.GetButtonDown("Jump") && isGrounded() && !isJumping)
            {
                // Add a vertical force to the Rigidbody2D
                jumpTimeCounter = 0f;
                isJumping = true;
            }

            if (Input.GetButton("Jump") && isJumping)
            {
                // Increase jump time counter
                jumpTimeCounter += Time.deltaTime;

                // Limit jump time to the maximum allowed
                jumpTimeCounter = Mathf.Clamp(jumpTimeCounter, 0f, playerData.maxJumpTime);
            }

            // Check if the jump button is released or max jump time is reached
            if ((Input.GetButtonUp("Jump") || jumpTimeCounter >= playerData.maxJumpTime) && isJumping)
            {
                jumpSound.Play();
                // Determine jump force based on jump time
                float jumpForce;

                if (jumpTimeCounter < playerData.maxJumpTime)
                {
                    jumpForce = playerData.shortJumpForce;
                }
                else
                {
                    jumpForce = playerData.longJumpForce;
                }

                // Apply jump force to the Rigidbody2D
                rb2d.velocity = new Vector2(rb2d.velocity.x, jumpForce);

                // Reset jump time counter
                jumpTimeCounter = 0f;
                isJumping = false;
            }



            //flip the sprite
            if (moveX > 0 && !facingRight) //face right
            {
                Flip();
            }
            if (moveX < 0 && facingRight) //face left
            {
                Flip();
            }

            // Change Weapon
            if (Input.GetKeyDown(KeyCode.F))
            {
                changeWeapon();
            }

            //Reload Weapon when it hits 0
            if (isReloading)
                return;

            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload());
            }


            //shoot weapon
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = mousePos - (Vector2)Gun.position;
            FaceMouse();

            if (Input.GetMouseButtonDown(0) && isGunActive)
            {
                StartCoroutine(ShootAfterAim());

            }

            //melee Attack
            if (Time.time >= nextAttackTime && isKnifeActive && Input.GetMouseButton(0)) // Check if knife is active and attack button is pressed
            {
                canDealDamage = true; // Set canDealDamage to true when the attack button is pressed
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
            //Reload Weapon when you hit R
            if (Input.GetKeyDown(KeyCode.R))
            {
                gunReloadSoundEffect.Play();
                StartCoroutine(Reload());
            }
        }

    }
    void FixedUpdate()
    {
        // Apply gravity with acceleration
        rb2d.velocity += Vector2.down * playerData.gravityScale * playerData.fallAcceleration * Time.fixedDeltaTime;
    }
    //This is the shoot method

    void FaceMouse()
    {
        Gun.transform.right = direction;
    }

    void shotSound()
    {
        gunSoundEffect.Play();
    }
    void shoot()
    {
        if (Time.time > nextFire)
        {
            if (currentAmmo > 0)
            {
                currentAmmo--;
                uiManagerReload.UpdateAmmo(currentAmmo);

                nextFire = Time.time + playerData.fireRate;
                GameObject BulletIns = Instantiate(Bullet, ShootPoint.position, ShootPoint.rotation);
                BulletIns.GetComponent<Rigidbody2D>().AddForce(BulletIns.transform.right * BulletSpeed);
            }
        }
    }

    public void TriggerShoot()
    {
        shoot();
    }
    IEnumerator ShootAfterAim()
    {
        aimingAnimation.SetBool("isAiming", true);

        // Wait for the aiming animation to finish
        yield return new WaitForSeconds(aimingAnimation.GetCurrentAnimatorStateInfo(0).length);

        // Trigger the shooting logic
        shoot();
    }
    //aim Gun animation

    public void FinishAimGun()
    {
        aimingAnimation.SetBool("isAiming", false);
    }
    //melee Attack
    void PlayMeleeSound()
    {
        meleeSoundEffect.Play();
    }

    // Add this method to your CharacterController2D class
    public void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyHealth>().TakeDamage(damage);
        }

        // Reset canDealDamage and animation state
        canDealDamage = false;
        meleeAttackAnimation.SetBool("isAttacking", false);
    }

    // Modify your Attack method as follows
    void Attack()
    {
        if (canDealDamage)
        {
            meleeAttackAnimation.SetBool("isAttacking", true);
        }
    }

    // Modify your DealDamageWithDelay coroutine to remove the call to ResetAnimationState
    IEnumerator DealDamageWithDelay()
    {
        // Wait for a short delay before dealing damage
        yield return new WaitForSeconds(delayBeforeDamage);

        if (canDealDamage)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.GetComponent<EnemyHealth>().TakeDamage(damage);
            }
        }

        // Reset canDealDamage (no need to reset animation state here)
        canDealDamage = false;
    }
    //Reload animation
    IEnumerator Reload()
    {
        if (!isGunActive || currentAmmo == playerData.maxAmmo)
        {
            // Do not reload if the gun is not active, or if the current ammo is full or zero
            yield break;
        }

        isReloading = true;

        reloadAnimation.SetBool("Reload", true);
        runningReloadAnimation.SetBool("Reload", true);

        yield return new WaitForSeconds(playerData.reloadTime);


        currentAmmo = playerData.maxAmmo;
        uiManagerReload.UpdateAmmo(currentAmmo);
        isReloading = false;
        reloadAnimation.SetBool("Reload", false);
        runningReloadAnimation.SetBool("Reload", false);
    }

    //Flip Sprite
    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    //Change Weapon
    void changeWeapon()
    {
        if (CurrentWeaponNo == 0)
        {
            // Deactivate previous weapon layer and activate the new one
            CurrentWeaponNo += 1;
            animator.SetLayerWeight(CurrentWeaponNo - 1, 0);
            animator.SetLayerWeight(CurrentWeaponNo, 1);


            isGunActive = false;
            isKnifeActive = true;
        }
        else
        {
            // Deactivate previous weapon layer and activate the new one
            CurrentWeaponNo -= 1;
            animator.SetLayerWeight(CurrentWeaponNo + 1, 0);
            animator.SetLayerWeight(CurrentWeaponNo, 1);

            isGunActive = true;
            isKnifeActive = false;

        }
    }

    //Check if the player is grounded
    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }
}