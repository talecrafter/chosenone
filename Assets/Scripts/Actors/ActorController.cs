﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorController : MonoBehaviour {

    // ================================================================================
    //  declarations
    // --------------------------------------------------------------------------------

    const float OFFSET_X = 0.7f;
    const float MELEE_RANGE_SQUARED = 0.2f;

    // ================================================================================
    //  public
    // --------------------------------------------------------------------------------

    public Actor actor { get; set; }

    public ActorController target = null;
    public GameObject displayObject;

    public bool directionRight = true;

    public FocusManager focusManager = null;

    public Vector2 speed = new Vector2(4.0f, 3.0f);

    // ================================================================================
    //  private
    // --------------------------------------------------------------------------------

    private FocusManager focusRight = null;
    private FocusManager focusLeft = null;
    private Transform _transform;

    private Timer _hitTimer = null;
    private Color _originalColor;

    // ================================================================================
    //  Unity methods
    // --------------------------------------------------------------------------------

    void Awake()
    {
        _transform = transform;

        focusRight = transform.Find("focusRight").GetComponent<FocusManager>();
        focusLeft = transform.Find("focusLeft").GetComponent<FocusManager>();

        UpdateFocusDirection();
    }

    void Start() {
        
    }

    void Update() {
        if (actor != null)
        {
            actor.Update();
        }

        // calculate enemy actions
        if (actor.faction != Actor.Faction.Player)
        {
            GetMovementTarget();

            if (actor.state == Actor.ActionState.Idle)
            {
                if (focusManager.focus != null && focusManager.focus.actor.faction != actor.faction)
                {
                    StandardAction();
                }
            }
            MoveToTarget();
        }

        // execute dead status
        if (actor.state == Actor.ActionState.Dead)
        {
            ShowHit(false);
            GetComponent<Collider2D>().enabled = false;
            this.enabled = false;

            if (actor.faction == Actor.Faction.Player)
            {
                GameMaster.Instance.PlayerDies();
            }
            else
            {
                GameMaster.Instance.EnemyDies();
            }
        }

        // timer for hit visuals
        if (_hitTimer != null)
        {
            _hitTimer.Update();

            if (_hitTimer.HasFinished())
            {
                ShowHit(false);
            }
        }

        // calculate z value
        UpdateDepth();
    }

    // ================================================================================
    //  public methods
    // --------------------------------------------------------------------------------

    public void SetMoveDirection(bool toTheRight)
    {
        if (toTheRight != directionRight)
        {
            flipDirection();
        }
    }

    public void flipDirection()
    {
        directionRight = !directionRight;

        // change visual avatar
        Transform t = displayObject.transform;
        t.localScale = new Vector3(t.localScale.x * -1.0f, t.localScale.y, t.localScale.z);

        UpdateFocusDirection();
    }

    public void SetActor(Actor actor)
    {
        this.actor = actor;
    }

    public void ApplyDamage(float d)
    {
        actor.ApplyDamage(d);
        ShowHit(true);
    }

    public void StandardAction()
    {
        Action action = actor.GetMainAbility();
        action.source = this;
        action.target = focusManager.focus;

        actor.TakeAction(action);
    }

    // ================================================================================
    //  private methods
    // --------------------------------------------------------------------------------

    private void MoveToTarget()
    {
        Vector3 targetPosition;

        if (target != null)
        {
            // move
            if (_transform.position.x > target.transform.position.x) // to the right
            {
                targetPosition = new Vector3(target.transform.position.x + OFFSET_X, target.transform.position.y, _transform.position.z);
            }
            else // to the left
            {
                targetPosition = new Vector3(target.transform.position.x - OFFSET_X, target.transform.position.y, _transform.position.z);
            }

            Debug.DrawLine(_transform.position, targetPosition);

            Vector3 moveDirection = targetPosition - _transform.position;

            if (moveDirection.sqrMagnitude > MELEE_RANGE_SQUARED)
            {
                moveDirection.Normalize();

                Vector3 movement = new Vector3(speed.x * moveDirection.x, speed.y * moveDirection.y, 0);
                movement *= Time.deltaTime;
                _transform.Translate(movement);

                if (targetPosition.x > _transform.position.x)
                    SetMoveDirection(true);
                if (targetPosition.x < _transform.position.x)
                    SetMoveDirection(false);
            }
        }
    }

    private void UpdateDepth()
    {
        _transform.position = new Vector3(_transform.position.x, _transform.position.y, _transform.position.y);
    }

    private void GetMovementTarget()
    {
        if (actor.faction != Actor.Faction.Player)
        {
            target = GameMaster.Instance.player;
        }
    }

    private void UpdateFocusDirection()
    {
        if (directionRight)
        {
            focusManager = focusRight;
        }
        else
        {
            focusManager = focusLeft;
        }
    }

    private void ShowHit(bool show)
    {
        if (show)
        {
            if (_hitTimer != null)
            {
                _hitTimer.Reset();
                return;
            }

            _hitTimer = new Timer(0.2f);
            _originalColor = displayObject.renderer.material.color;
            displayObject.renderer.material.color = new Color(1.0f, 0.4f, 0.4f);
        }
        else
        {
            if (_hitTimer != null)
            {
                displayObject.renderer.material.color = _originalColor;
                _hitTimer = null;
            }
        }
    }

}