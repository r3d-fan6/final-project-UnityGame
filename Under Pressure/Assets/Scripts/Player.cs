﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    //Dylans code
    #region Dylans Variables
    // Importing the movement script.
    public Movement movement;
    public GameManager gameManager;

    private bool at_locker = false;
    public bool locker_done = false;

    private bool crowdCoroutine = false;
    #endregion

    // Ethans code
    #region Ethans Variables
    public int crowd_panic_rate = 2;        //Controls the rate at which the crows enemy raises
    public int pick_up_restore = 14;
    public int salesman_panic_rate = 2;
    public int anxiety = 10;

    public Text anxiety_text;           //Stores the anxiety ratio text.
    public GameObject breathing_icon;
    public GameObject music_icon;
    public GameObject stress_icon;

    [SerializeField] private health_bar_controller healthBar;

    private bool stress_ball_obtained = false;  //checks to see if the player has obtained the stress ball
    private bool breathing = false;             //checks to see if the player is currently breathing
    private bool music_ready = true;           //checks to see if the music player is on cooldown

    //check to see that all the tasks in the first level have been completed.
    private bool check_one_ready = false;
    private bool check_two_ready = false;
    private bool check_three_ready = false;
    private bool check_four_ready = false;

    //check to see if the player is in the appropiate zones.
    private bool in_zone_one = false;
    private bool in_zone_two = false;
    private bool in_zone_three = false;
    private bool in_zone_four = false;

    //inform player that they can complete errand
    public GameObject errandInformer;
    private bool in_zone = false;
    public GameObject exitinformer;
    private bool in_exit = false;

    public GameObject lockerInformer;

    //ready to leave
    private bool exit_ready = false;

    //animations
    public Animator animator;
    #endregion

    void Start()
    {
        Debug.Log(errandInformer);
        if (errandInformer == null) errandInformer = null;
        if (exitinformer == null) exitinformer = null;
        SetAnxietyText();
        InvokeRepeating("anxietyPerSecond", 0.0f, 1.0f);
    }

    // Taken from Ethans code, but modified to reduce line count.
    #region Ethans update method
    void Update()
    {
        //ABILITY KEY INPUTS
        if (Input.GetKey("z") && !breathing)
        {
            StartCoroutine("breathing_routine");
        }
        else if (Input.GetKeyUp("z") && breathing)
        {
            StopCoroutine("breathing_routine");
            breathing = false;
        }

        if (Input.GetKeyDown("x") && music_ready)
        {
            music_ready = false;
            anxiety -= 20;
            Invoke("music_cooldown_complete", 5);
            SetAnxietyText();
        }

        //Check First Level Tasks
        if (Input.GetKeyDown("c") && in_zone_one) check_one_ready = true;
        else if (Input.GetKeyDown("c") && in_zone_two) check_two_ready = true;
        else if (Input.GetKeyDown("c") && in_zone_three) check_three_ready = true;
        else if (Input.GetKeyDown("c") && in_zone_four) check_four_ready = true;

        if (Input.GetKeyDown("c") && at_locker) locker_done = true;

        //Finish first level
        if (Input.GetKeyDown("c") && check_one_ready && check_two_ready && check_three_ready && check_four_ready && exit_ready) gameManager.CompletedLevel();

        //UI INFORMATION
        if (stress_ball_obtained) stress_icon.SetActive(true);
        else if (!stress_ball_obtained) stress_icon.SetActive(false);

        if (!music_ready) music_icon.SetActive(false);

        if (breathing) breathing_icon.SetActive(false);
        else if (!breathing) breathing_icon.SetActive(true);

        //END THE GAME IF ANXIETY REACHES 100
        //ALSO PREVENTS ANXIETY FROM GOING OVER 100
        if (anxiety >= 100)
        {
            anxiety = 100;
            SetAnxietyText();
            FindObjectOfType<GameManager>().EndGame();
        }

        if (at_locker) lockerInformer.SetActive(true);
        else lockerInformer.SetActive(false);

        if (in_zone) errandInformer.SetActive(true);
        else errandInformer.SetActive(false);

        if (check_one_ready && check_two_ready && check_three_ready && check_four_ready && exit_ready) exitinformer.SetActive(true);
        else exitinformer.SetActive(false);

        #region ANIMATIONS
            //Animate the player on key presses

            if (Input.GetKeyDown("w"))
            {
                animator.SetBool("up", true);
            }
            else if (Input.GetKeyUp("w"))
            {
                animator.SetBool("up", false);
            }

            if (Input.GetKeyDown("a"))
            {
                animator.SetBool("left", true);
            }
            else if (Input.GetKeyUp("a"))
            {
                animator.SetBool("left", false);
            }

            if (Input.GetKeyDown("s"))
            {
                animator.SetBool("down", true);
            }
            else if (Input.GetKeyUp("s"))
            {
                animator.SetBool("down", false);
            }

            if (Input.GetKeyDown("d"))
            {
                animator.SetBool("right", true);
            }
            else if (Input.GetKeyUp("d"))
            {
                animator.SetBool("right", false);
            }

            #endregion
    }

    #endregion

    #region Dylans FixedUpdate() so the player can move.
    void FixedUpdate()
    {
        if (!breathing) // Taken from Ethans code, don't run the code inside if the player is breathing.
        {
            // Fixed directional input.
            if (Input.GetKey("w"))
            {
                movement.MoveUp();
            }

            if (Input.GetKey("s"))
            {
                movement.MoveDown();
            }

            if (Input.GetKey("d"))
            {
                movement.MoveRight();
            }

            if (Input.GetKey("a"))
            {
                movement.MoveLeft();
            }

            // Diagonal movement input.
            if (Input.GetKey("w") && Input.GetKey("d"))
            {
                movement.UpRight();
            }

            if (Input.GetKey("w") && Input.GetKey("a"))
            {
                movement.UpLeft();
            }

            if (Input.GetKey("s") && Input.GetKey("d"))
            {
                movement.DownRight();
            }

            if (Input.GetKey("s") && Input.GetKey("a"))
            {
                movement.DownLeft();
            }
        }
    }
    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("good_pickup"))
        {
            anxiety -= pick_up_restore;
            Destroy(other.gameObject);
        }

        if (other.gameObject.CompareTag("stress_ball"))
        {
            anxiety -= pick_up_restore;
            Destroy(other.gameObject);
            SetAnxietyText();
        }

        if (other.gameObject.CompareTag("crowd"))
        {
            crowdCoroutine = true;
            StartCoroutine("crowd_panic");
        }

        if (other.gameObject.CompareTag("bad_pickup"))
        {
            anxiety -= pick_up_restore;
            Destroy(other.gameObject);
            Invoke("unhealthy_pickup", 3);
        }

        if (other.gameObject.CompareTag("salesman")) StartCoroutine("salesman_panic");
        if (other.gameObject.CompareTag("friends")) StartCoroutine("friends_panic");
        if (other.gameObject.CompareTag("crush"))
        {
            if (crowdCoroutine)
            {
                StopCoroutine("crowd_panic");
                StartCoroutine("crush_panic");
            } else
            {
                StartCoroutine("crush_panic");
            }
        }

        if (other.gameObject.CompareTag("bully")) anxiety += 40;

        if (other.gameObject.CompareTag("check_point_one")) in_zone_one = true; in_zone = true;
        if (other.gameObject.CompareTag("check_point_two")) in_zone_two = true; in_zone = true;
        if (other.gameObject.CompareTag("check_point_three")) in_zone_three = true; in_zone = true;
        if (other.gameObject.CompareTag("check_point_four")) in_zone_four = true; in_zone = true;
        if (other.gameObject.CompareTag("exit_door")) exit_ready = true;

        if (other.gameObject.CompareTag("locker")) at_locker = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        //Check through to see what the player is colliding with
        //And also begin the code for each seperate pickup / enemy
        if (other.gameObject.CompareTag("crowd"))
        {
            crowdCoroutine = false;
            StopCoroutine("crowd_panic");
        }
        if (other.gameObject.CompareTag("salesman")) StopCoroutine("salesman_panic");
        if (other.gameObject.CompareTag("crush")) StopCoroutine("crush_panic");
        if (other.gameObject.CompareTag("friends")) StopCoroutine("Friends_panic");
        if (other.gameObject.CompareTag("check_point_one")) in_zone_one = false; in_zone = false;
        if (other.gameObject.CompareTag("check_point_two")) in_zone_two = false; in_zone = false;
        if (other.gameObject.CompareTag("check_point_three")) in_zone_three = false; in_zone = false;
        if (other.gameObject.CompareTag("check_point_four")) in_zone_four = false; in_zone = false;
        if (other.gameObject.CompareTag("exit_door")) exit_ready = false;

        if (other.gameObject.CompareTag("locker")) at_locker = false;
    }

    #region Ethans functions
    #region Functions for the UI and pickups.
    void music_cooldown_complete()
    {
        music_ready = true;
        music_icon.SetActive(true);
    }

    void unhealthy_pickup()
    {
        anxiety += 10;
        SetAnxietyText();
    }

    void SetAnxietyText()
    {
        if (anxiety < 0) anxiety = 0;
        anxiety_text.text = anxiety.ToString() + "%";
    }

    void anxietyPerSecond()
    {
        anxiety++;
        SetAnxietyText();
    }
    #endregion

    #region IEnumerators
    IEnumerator breathing_routine()
    {
        for (int current_anxiety = anxiety; current_anxiety >= 1; current_anxiety -= 6)
        {
            breathing = true;
            anxiety = current_anxiety;

            SetAnxietyText();
            yield return new WaitForSeconds(.3f);
        }
    }

    IEnumerator crowd_panic()
    {
        for (int current_anxiety = anxiety; current_anxiety <= 100; current_anxiety += crowd_panic_rate)
        {
            anxiety = current_anxiety;
            SetAnxietyText();
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator salesman_panic()
    {
        for (int current_anxiety = anxiety; current_anxiety <= 100; current_anxiety += salesman_panic_rate)
        {
            anxiety = current_anxiety;
            SetAnxietyText();
            yield return new WaitForSeconds(0.8f);
        }
    }

    IEnumerator boss_panic()
    {
        for (int current_anxiety = anxiety; current_anxiety <= 100; current_anxiety += 2)
        {
            anxiety = current_anxiety;
            SetAnxietyText();
            yield return new WaitForSeconds(.3f);
        }
    }

    //dictate behaviour for when player is within crush hitbox
    IEnumerator crush_panic()
    {
        for (int current_anxiety = anxiety; current_anxiety <= 100; current_anxiety += 4)
        {
            anxiety = current_anxiety;
            SetAnxietyText();
            yield return new WaitForSeconds(0.6f);
        }
    }
    #endregion
    #endregion
}