using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerGrapple : MonoBehaviour
{
    Player player;
    Rigidbody2D rb;
    LineRenderer lineRenderer;
    AudioSource grappleAudioSource;
    GameObject grappleBullet;
    GameObject grappleParent;
    GameObject grappleFunctionPoint;
    Vector2 grapplePoint;
    PublicTimer grappleCDtimer = new PublicTimer(30f);
    bool canGrapple;
    bool grappleIsShooting;
    bool isGrappling;
    bool initialized;
    float grappleAudioActiveTime;
    float maxGrappleSpeed = 15f;

    public bool IsGrappling => isGrappling;

    public void Initialize(Player owner)
    {
        player = owner;

        if(player == null || player.grapple == null || player.grapple.gameObject == null)
            return;

        rb = player.GetComponent<Rigidbody2D>();
        lineRenderer = player.GetComponent<LineRenderer>();

        if(lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            float width = 0.015f;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.enabled = false;
        }

        AudioSource[] playerAudioSources = player.GetComponents<AudioSource>();
        if(playerAudioSources.Length > 0)
        {
            grappleAudioSource = playerAudioSources[0];
            grappleAudioSource.loop = true;
        }

        Transform grappleTransform = player.grapple.gameObject.transform;
        if(grappleTransform.childCount > 3)
            grappleFunctionPoint = grappleTransform.GetChild(3).gameObject;

        GameObject bulletObject = GameObject.FindGameObjectWithTag("GrappleBullet");
        if(bulletObject != null)
        {
            grappleBullet = bulletObject;
            grappleBullet.SetActive(false);
        }

        initialized = rb != null && lineRenderer != null && grappleFunctionPoint != null && grappleBullet != null;
    }

    public void TickCooldown()
    {
        if(!canGrapple && grappleCDtimer.TickLoop())
            canGrapple = true;
    }

    public void TryShoot()
    {
        if(!initialized || !canGrapple)
            return;

        Vector2 dir = player.AimWorld - (Vector2)player.transform.position;
        float distance = 50f;
        RaycastHit2D hit = Physics2D.Raycast(player.transform.position, dir, distance, player.GroundLayer);

        if(hit == false)
            return;

        SoundManager.PlaySound(0.5f,0.2f,"grapple");
        isGrappling = true;
        grapplePoint = hit.point;
        grappleBullet.SetActive(true);
        grappleBullet.transform.position = grappleFunctionPoint.transform.position;
        grappleParent = hit.collider.gameObject;
        rb.gravityScale = 0f;
        lineRenderer.enabled = true;
        canGrapple = false;
        grappleCDtimer.Reset();
        grappleIsShooting = true;
    }

    public void MovementUpdate()
    {
        if(!initialized || !isGrappling)
            return;

        Vector2 dir = grapplePoint - (Vector2)grappleFunctionPoint.transform.position;

        UpdateGrapplePullAudio(!grappleIsShooting);

        if(grappleIsShooting)
            GrappleShootingUpdate(dir);
        else
            GrappleUpdate(dir);

        lineRenderer.SetPosition(0,grappleFunctionPoint.transform.position);
        lineRenderer.SetPosition(1,grappleBullet.transform.position);
    }

    public void Cancel()
    {
        if(rb == null)
            return;

        isGrappling = false;
        grappleIsShooting = false;
        UpdateGrapplePullAudio(false);
        rb.gravityScale = player.InitialGravityScale;

        if(lineRenderer != null)
            lineRenderer.enabled = false;

        if(grappleBullet != null)
            grappleBullet.SetActive(false);
    }

    public void StopPullAudio()
    {
        UpdateGrapplePullAudio(false);
    }

    void OnDisable()
    {
        UpdateGrapplePullAudio(false);
    }

    void GrappleShootingUpdate(Vector2 dir)
    {
        float grappleShootSpeed = 0.1f;
        grappleBullet.transform.Translate(dir * grappleShootSpeed);
        if(((Vector2)grappleBullet.transform.position - grapplePoint).magnitude < 0.1f)
        {
            grappleIsShooting = false;
            grappleBullet.transform.SetParent(grappleParent.transform);
        }
    }

    void GrappleUpdate(Vector2 dir)
    {
        float grappleSpeed = 17f;
        Vector2 fakeGravity = Vector2.down * 8f;

        rb.AddForce(dir.normalized * grappleSpeed + fakeGravity);
        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity,maxGrappleSpeed);

        if(dir.magnitude < 1f)
            Cancel();
    }

    void UpdateGrapplePullAudio(bool isBeingPulled)
    {
        const float grappleAudioBasePitch = 1f;
        const float grappleAudioPitchRange = 0.35f;
        const float grappleAudioPitchCycleSpeed = 2.5f;

        if(grappleAudioSource == null)
            return;

        if(!isBeingPulled)
        {
            grappleAudioActiveTime = 0f;
            grappleAudioSource.pitch = grappleAudioBasePitch;

            if(grappleAudioSource.isPlaying)
                grappleAudioSource.Stop();

            return;
        }

        grappleAudioActiveTime += Time.deltaTime;

        float halfRange = grappleAudioPitchRange * 0.5f;
        float pitchOffset = Mathf.PingPong(grappleAudioActiveTime * grappleAudioPitchCycleSpeed, grappleAudioPitchRange) - halfRange;
        grappleAudioSource.pitch = grappleAudioBasePitch + pitchOffset;

        if(!grappleAudioSource.isPlaying)
            grappleAudioSource.Play();
    }
}