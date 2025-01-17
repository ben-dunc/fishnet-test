using System.Collections;
using System.Collections.Generic;
using FishNet.CodeGenerating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using LiteNetLib;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] float moveStrength = 1;
    [SerializeField] Rigidbody2D rb2;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Animator animator;
    [SerializeField] ParticleSystem boostFX;
    [SerializeField] TMP_Text pingTMP;

    [System.NonSerialized, AllowMutableSyncType] SyncVar<int> animationIndex = new();
    [System.NonSerialized, AllowMutableSyncType] SyncVar<long> pingMs = new();
    [System.NonSerialized, AllowMutableSyncType] SyncVar<bool> spriteFlip = new();

    float boostCoolDownTimeRef = 0f;
    Transform _trans;

    float horzDir = 0;
    float horzDir_prev = 0;
    float vertDir = 0;
    float vertDir_prev = 0;

    void Awake()
    {
        animationIndex.Value = -1;
        pingMs.Value = 0;
        spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        _trans = transform;
    }

    public override void OnStartClient()
    {
        animationIndex.OnChange += (int prev, int next, bool asServer) => animator.SetInteger("PlayerColor", next);
        spriteFlip.OnChange += (bool prev, bool next, bool asServer) => spriteRenderer.flipX = !next;
        pingMs.OnChange += (long prev, long next, bool asServer) => pingTMP.text = $"{next}";

        if (base.IsOwner && base.IsClientInitialized)
            InvokeRepeating("UpdatePing_Client", 1, 1);
    }

    public override void OnStartServer()
    {
        animationIndex.Value = Random.Range(0, 4);
        if (base.IsServerInitialized)
            gameObject.name += " (SERVER)";
    }

    void Update()
    {
        // client only
        if (!base.IsOwner)
            return;

        // controls
        vertDir = 0;
        horzDir = 0;

        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
            vertDir++;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            vertDir--;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
            horzDir++;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
            horzDir--;

        if (Input.GetMouseButton(0))
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = _trans.position.z;
            Vector3 mouseDirection = (mousePosition - _trans.position).normalized;
            horzDir += mouseDirection.x;
            vertDir += mouseDirection.y;
        }

        if (vertDir != vertDir_prev || horzDir != horzDir_prev)
            OnPlayerDirChange_Server(horzDir, vertDir);

        vertDir_prev = vertDir;
        horzDir_prev = horzDir;

        // boost
        if (boostCoolDownTimeRef <= 0 && Input.GetKeyDown(KeyCode.Space) && (vertDir != 0 || horzDir != 0))
        {
            DoBoost_Server();
            boostCoolDownTimeRef = 0.5f;
        }
        else
            boostCoolDownTimeRef -= Time.deltaTime;

        // change color
        if (Input.GetKeyDown(KeyCode.E))
            CycleFishColor_Server();
    }

    void FixedUpdate()
    {
        // server only
        if (base.IsServerInitialized)
        {
            var deltaedSpeed = moveStrength * Time.fixedDeltaTime;
            rb2.AddForce(new Vector2(horzDir * deltaedSpeed, vertDir * deltaedSpeed));
        }
    }

    [Client]
    void UpdatePing_Client()
    {
        long ping = (int)TimeManager.RoundTripTime;
        long deduction = 0;

        ping = (long)Mathf.Max(1, ping - deduction);

        SetClientPing_Server(ping);
    }

    [ServerRpc]
    void OnPlayerDirChange_Server(float h, float v)
    {
        horzDir = h;
        vertDir = v;

        if (h != 0)
            spriteFlip.Value = h < 0;
    }

    [ServerRpc]
    void DoBoost_Server()
    {
        var deltaedSpeed = moveStrength * Time.fixedDeltaTime;
        rb2.AddForce(new Vector2(horzDir * deltaedSpeed, vertDir * deltaedSpeed), ForceMode2D.Impulse);
        OnBoostFX_Observer(new Vector2(-rb2.velocity.x, -rb2.velocity.y));
    }

    [ServerRpc]
    void CycleFishColor_Server()
    {
        var prevIndex = animationIndex.Value;
        animationIndex.Value = (prevIndex + 1) % 4;
    }

    [ServerRpc]
    void SetClientPing_Server(long pingMsClient)
    {
        pingMs.Value = pingMsClient;
    }

    [ObserversRpc]
    void OnBoostFX_Observer(Vector2 dir)
    {
        if (boostFX != null)
        {
            var e = boostFX.transform.eulerAngles;
            e.z = Vector3.SignedAngle(Vector3.up, new Vector3(dir.x, dir.y, 0), Vector3.forward);
            boostFX.transform.eulerAngles = e;

            boostFX.Play();
        }
    }
}
