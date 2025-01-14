﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//TMPro for Damage Numbers
using TMPro;

public class Player : MonoBehaviour
{

    #region Attack_vars
    [SerializeField]
    [Tooltip("the time untill the hitbox is created")]
    private float hitboxtiming;
    [SerializeField]
    private float attack_range;
    [SerializeField]
    [Tooltip("amount of damage basic attack deals to enemy")]
    private int dmg;
    private float attack_timer;
    private float attackPointDist;

    // holds status/power of the player (gained from collecting crystal)
    public enum power {None, Fire, Ice}; // add more...
    power status = power.None;

    // things added for flamethrower class !
    private float attacktimer;

    [SerializeField]
    private Flamethrower flames;
    private float flameCD = 1f;
    private bool flExist;
    public bool lastdir;

    // things added for ice atk
    [SerializeField]
    private Icicle ice;
    private float iceCD = 0.5f;
    #endregion

    #region UI/Health_vars
    [SerializeField]
    [Tooltip("maximum health player can have")]
    private int max_health;
    private int curr_health;
    [SerializeField]
    private Slider HpBar;
    #endregion

    #region Unity_vars
    private Rigidbody2D PlayerRB;
    [SerializeField]
    private GameManager gm;
    [SerializeField]
    private int currlevel;
    #endregion

    #region Movement_vars
    [SerializeField]
    [Tooltip("Character move speed")]
    private float move_speed;
    [SerializeField]
    [Tooltip("Velocity of character jump")] //may need to add a jump duration in addition to this
    private float jump_vel;
    [Tooltip("tells the player if the can jump or not")] // may need to move some colliders/ children
    public bool can_jump; // may need to change to an int if we want double jumps
    private float x_input;
    private float y_input; // may get rid of this depending on controll scheme
    [SerializeField]
    [Tooltip("How long we want to be jumping")]
    private float jump_len;
    private float curjump_len;
    private float jump_timer;
    private float vert_vel;
    private Vector2 cur_direction;
    private IEnumerator jumped = null;
    
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;

    [SerializeField]
    [Tooltip("rate at which it grows")]
    private float growthRate;

    #endregion

    #region Unity_funcs
    // Start is called before the first frame update
    void Awake()
    {
        PlayerRB = GetComponent<Rigidbody2D>();
        vert_vel = 0;
        curr_health = max_health;
        attack_timer = 0f;
        attackPointDist = attackPoint.transform.position.x - this.transform.position.x;
        //flamethrwer
        flExist = false;
        HpBar.value = 1;
    }

    // Update is called once per frame
    void Update()
    {
        x_input = Input.GetAxisRaw("Horizontal");
        y_input = Input.GetAxisRaw("Vertical");
        move();
        switchAttackPoint();
        if (Input.GetKeyDown("space"))
        {
            if (curr_size > 0) {
                curr_size--;
                this.transform.localScale = new Vector3(this.transform.localScale.x - growthRate, this.transform.localScale.y - growthRate, this.transform.localScale.z - growthRate);
            }
        }
        if( attack_timer<= 0  && ( Input.GetKeyDown("i") || Input.GetKeyDown("z")))
        {
            //StartCoroutine(Attack());
            Attack();
        } else if (attack_timer > 0)
        {
            attack_timer -= Time.deltaTime;
        }

        //also for flamethrower cuz im confused
        if (attacktimer > 0)
        {
            attacktimer -= Time.deltaTime;
        }
        if (Input.GetKeyDown("k") && attacktimer <= 0) {
            if (status == power.Fire)
            {
                attacktimer = flameCD;
                Flame();
            } else if (status == power.Ice) 
            {
                attacktimer = iceCD;
                Ice();
            }
        }
    }
    #endregion

    #region Move_funcs
    private void move()
    {
        if(y_input > 0 && can_jump)
        {
            jumped = jumping();
            can_jump = false;
            StartCoroutine(jumped);
            //vert_vel = jump_vel;
        } else if ( y_input <= 0 && !can_jump && jumped !=null)
        {
            curjump_len = jump_timer + .5f;
            jumped = null;
        }
        if(x_input != 0)
        {
            cur_direction = new Vector2(x_input / x_input, 0);
            PlayerRB.velocity = new Vector2(x_input * move_speed, vert_vel);
            lastdir = (x_input > 0);
        }
        else
        {
            PlayerRB.velocity = new Vector2(PlayerRB.velocity.x, vert_vel);
        }
    }

    // may need to think about how to cancell this function if we want dashes
    // may need to think about slowing the jump down near the end so it isn't so abrupt
    IEnumerator jumping() 
    {
        can_jump = false;
        //float jump_timer = jump_len;
        jump_timer = 0;
        curjump_len = jump_len;
        while(jump_timer < curjump_len)
        {
            vert_vel = Mathf.Lerp(jump_vel, 0, jump_timer / curjump_len);
            jump_timer += Time.deltaTime;
            yield return null;
        }
        jump_timer = 0;
       // while (jump_timer < jump_len && !can_jump)
        //{
        //    vert_vel = Mathf.Lerp(0, -jump_vel, jump_timer / jump_len);
        //    jump_timer += Time.deltaTime;
        //    yield return null;
        //}
        vert_vel = 0;
    }
    #endregion

    #region Attack_funcs
    // IEnumerator Attack()
    // {
    //     yield return new WaitForSeconds(hitboxtiming);
    //     RaycastHit2D[] hits = Physics2D.BoxCastAll(PlayerRB.position + cur_direction, new Vector2(attack_range, attack_range), 0f, Vector2.zero);

    //     foreach (RaycastHit2D hit in hits)
    //     {
    //         
    //     }
    //     yield return new WaitForSeconds(hitboxtiming);
    // }
    void Attack() {
       // yield return new WaitForSeconds(hitboxtiming);
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D hit in hits) {
            if (hit.transform.CompareTag("Enemy")) {
                hit.transform.GetComponent<Enemy>().TakeDamage(dmg);
            }
        }
      //  yield return new WaitForSeconds(hitboxtiming);
    }

    void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    void switchAttackPoint() {
        Vector3 position = attackPoint.transform.position;
    if (Input.GetKeyDown("a")) {
        position.x = this.transform.position.x - attackPointDist;
    } else if (Input.GetKeyDown("d")) {
        position.x = this.transform.position.x + attackPointDist;
    }
        attackPoint.transform.position = position;
    }

    private void Flame()
    {
        Flamethrower.dir = lastdir;
        float s = curr_size;
        Instantiate(flames, this.transform.position + new Vector3(2.5f + s*10f, -.5f, 0), this.transform.rotation);
        //you need to make a bullet before you make attack ideally
        //set the bulets velocity
        //look up instantaite
    }

    private void Ice()
    {
        if (lastdir) {
            Instantiate(ice, this.transform.position + new Vector3(1, 1, 0).normalized, this.transform.rotation).transform.Rotate(0,0,30);
            Instantiate(ice, this.transform.position + new Vector3(1, 0, 0), this.transform.rotation).transform.Rotate(0,0,0);
            Instantiate(ice, this.transform.position + new Vector3(1, -1, 0).normalized, this.transform.rotation).transform.Rotate(0,0,-30);
        } else {
            Instantiate(ice, this.transform.position + new Vector3(-1, 1, 0).normalized, this.transform.rotation).transform.Rotate(0,0,-30);
            Instantiate(ice, this.transform.position + new Vector3(-1, 0, 0), this.transform.rotation).transform.Rotate(0,0,0);
            Instantiate(ice, this.transform.position + new Vector3(-1, -1, 0).normalized, this.transform.rotation).transform.Rotate(0,0,30);
        }
    }

    #endregion

    #region Size_func

    public int curr_size;
    [SerializeField]
    [Tooltip("Maximum size a player can carry")]
    private int max_size;

    public void size_up()
    {
        if (curr_size < max_size) {
            curr_size++;
            float scale = curr_size * growthRate;
            this.transform.localScale = new Vector3(1.5f + scale, 1.5f + scale, 1f + scale);
        }

    }

    #endregion


    #region health_func
    [SerializeField]
    GameObject damageTextPrefab;
    private int healthPickUp = 3;

    public void TakeDamage(int dmg)
    {
        curr_health -= dmg;
        float tempcur =  curr_health;
        float tempmax = max_health;
        HpBar.value =  tempcur /  tempmax;
        GameObject DamageTextInstance = Instantiate(damageTextPrefab, this.transform.position, Quaternion.identity, this.transform);
        DamageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(dmg.ToString());
        if (curr_health <= 0)
        {
            Destroy(this.gameObject);
            if (currlevel == 0)
            {
                gm.TutorialLevel();
            } else if(currlevel == 1)
            {
                gm.JaneLevel();
            }
            else
            {
                gm.AbbyLevel();
            }
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject item = other.gameObject;

        if(item.tag == "fireCrystal")
        {
            status = power.Fire; 
            Debug.Log(status);
            Destroy(item);
        }

        if(item.tag == "iceCrystal")
        {
            status = power.Ice; 
            Debug.Log(status);
            Destroy(item);
        }

        if(item.tag == "sizeCrystal")
        {
            size_up();
            Destroy(item);
        }

        if (item.tag == "heart") {
            int tempHealth = healthPickUp + curr_health;
            if (tempHealth > max_health) {
                curr_health = max_health;
            } else {
                curr_health = tempHealth;
            }
            float tempcur =  curr_health;
            float tempmax = max_health;
            HpBar.value =  tempcur /  tempmax;
            Destroy(item);
        }

        // Repeat if statement for different type of crystals/droplets
    }


    public Player.power getPowerStatus() {
        return status;
    }
}
