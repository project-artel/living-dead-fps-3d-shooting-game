using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class player_health : MonoBehaviour
{
    public float health=100f;
    bool isPlayingAnim=true;
    private float max_health=100f;
    public bool player_dead=false;

#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    internal bool QaInvincible { get; set; }
    private int qaMedkitInputCount;
    private string qaLastMedkitInput = "null";

    internal string ReadQaMedkitState() =>
        System.FormattableString.Invariant($"{{\"health\":{health:R},\"input_count\":{qaMedkitInputCount},\"last_input\":{qaLastMedkitInput}}}");
#endif

    // public float cooldown_time=0f;
    // public float cooling_rate=5f;
    // public float recharge_health_f=3f;
    public Image hurt_image1,hurt_image2,hurt_image3;
    public AudioSource audioSource;
	public AudioClip player_hurt;
    public AudioClip player_dying;
    public health_bar_script hbs;
    private GameObject[] medikits;
    private Transform player;
    private GameObject used_kit;
    bool recharge_health;
    // public bool is_attack_player=false;

    public void TakeDamage(float amount){
#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (QaInvincible)
            return;
#endif
        if(health>0){
         health-=amount;
        //  is_attack_player=true;
         audioSource.PlayOneShot(player_hurt);
        }
         else  if(health<=0){
            // is_attack_player=false;
             StartCoroutine(PlayAnim());
         }
    }
    private void Die(){
        Destroy(gameObject);
    }

private IEnumerator PlayAnim () {
    isPlayingAnim = true;
    audioSource.PlayOneShot(player_dying);
    yield return new WaitForSeconds(3f);
    player_dead=true;
    isPlayingAnim = false;
}
 
 void hurt_image_fn(){
     Color red_scatter=hurt_image1.color;
     red_scatter.a=1-(health/max_health);
     hurt_image1.color=red_scatter;
     hurt_image2.color=red_scatter;
     hurt_image3.color=red_scatter;
 }

// Called every frame
void Update () {
    hurt_image_fn();
    hbs.setmaxhealth(max_health);
    hbs.sethealth(health);
    player_hurt=Resources.Load<AudioClip> ("player_hurt");
    player_dying=Resources.Load<AudioClip> ("player_dying");
    player= GameObject.FindGameObjectWithTag("Player").transform;
	medikits= GameObject.FindGameObjectsWithTag("medikit");
    // if(health>0 && health+recharge_health_f<=100 && Time.time>cooldown_time && is_attack_player==false){
    //     health+=recharge_health_f;
    //     cooldown_time=Time.time+cooling_rate;
    // }

    bool medkitKeyDown = Input.GetKeyDown(KeyCode.CapsLock);
#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    float healthBeforeInput = health;
    float nearestMedkitDistance = float.PositiveInfinity;
    int? consumedMedkitId = null;
#endif

    foreach (var kit in medikits)
    {
        float distance = Vector3.Distance(player.position, kit.transform.position);
#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        if (medkitKeyDown)
            nearestMedkitDistance = Mathf.Min(nearestMedkitDistance, distance);
#endif
        if (distance < 2.5f && health != max_health && medkitKeyDown)
        {
            used_kit = kit;
            recharge_health = true;
        }
    }

    if (recharge_health)
    {
        health = 100f;
#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
        consumedMedkitId = used_kit.GetInstanceID();
#endif
        Destroy(used_kit);
        recharge_health = false;
    }

#if ARTEL_GAME_FUNCTIONS && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    if (medkitKeyDown)
    {
        qaMedkitInputCount++;
        string distance = medikits.Length == 0
            ? "null"
            : System.FormattableString.Invariant($"{nearestMedkitDistance:R}");
        string consumedId = consumedMedkitId.HasValue
            ? System.FormattableString.Invariant($"{consumedMedkitId.Value}")
            : "null";
        qaLastMedkitInput = System.FormattableString.Invariant(
            $"{{\"frame\":{Time.frameCount},\"medkit_count\":{medikits.Length},\"nearest_distance_m\":{distance},\"health_before\":{healthBeforeInput:R},\"health_after\":{health:R},\"consumed_medkit_id\":{consumedId}}}");
    }
#endif

    // if (health<=0 && isPlayingAnim==false) {
    //     audioSource.Pause();
    //     Invoke("Die",0);
    // }
}
}
