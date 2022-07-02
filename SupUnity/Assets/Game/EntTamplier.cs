using UnityEngine;
using DG.Tweening;
using GameLib.Random;

public class EntTamplier : MonoBehaviour
{
    public Material InfectionMaterial;

    const float ThrowPodHeight = 120f;
    const float ThrowPodDuration = 2.5f;

    public Rect LivingRect;
    private Vector3 _wayPoint1;
    private Vector3 _wayPoint2;
    private Vector3 _currentPoint;
    public Range MoveTime;
    private float _margin = 1.8f;

    public Transform[] Spores;
    public Transform[] Rays;
    public bool IsInfected;
    public bool IsExpoding;
    public Rotating Rotating;
    private Tweener _movingTweener;

    void Start()
    {
        // randomize
        foreach (var spore in Spores)
        {
            spore.gameObject.SetActive(Random.value < 0.5f);
        }
        if(IsInfected) // for the node which is infected on start
            Spores[0].gameObject.SetActive(true);

        if (LivingRect.width > LivingRect.height) // move horizontally
        {
            _wayPoint1 = LivingRect.center + Vector2.left * LivingRect.width * 0.5f;
            _wayPoint2 = LivingRect.center + Vector2.right * LivingRect.width * 0.5f;
            _wayPoint1.x += _margin;
            _wayPoint2.x -= _margin;
        }
        else // move vertically 
        {
            _wayPoint1 = LivingRect.center + Vector2.up * LivingRect.height * 0.5f;
            _wayPoint2 = LivingRect.center + Vector2.down * LivingRect.height * 0.5f;
            _wayPoint1.y -= _margin;
            _wayPoint2.y += _margin;
        }
        _currentPoint = Random.value < 0.5f ? _wayPoint1 : _wayPoint2;
        NextMove();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsInfected)
            return;

        var pod = other.gameObject.GetComponent<Pod>();
        if (pod == null)
            return;

        Infect(pod.OwnerId);
        Destroy(pod.gameObject);
    }

    public void Infect(int donorID)
    {
        IsInfected = true;
        if (donorID != -1)
            Progression.Instance.IncScore();

        // Activate aiming lines
        foreach (var spore in Spores)
        {
            if( spore.gameObject.activeSelf )
                spore.transform.GetChild(0).gameObject.SetActive(true);
        }

        // Change color
        GetComponent<MeshRenderer>().material = InfectionMaterial;
        foreach (var spore in Spores)
            spore.GetComponent<MeshRenderer>().material = InfectionMaterial;

        Rotating.StartRotating();
    }

    private void NextMove()
    {
        _movingTweener = transform.DOMove(
            _currentPoint == _wayPoint1 ? _wayPoint2 : _wayPoint1,
            Random.Range(MoveTime.From, MoveTime.To)).SetEase(Ease.Linear)
            .OnComplete(OnCompleteMove);
    }

    private void OnCompleteMove()
    {
        _currentPoint = _currentPoint == _wayPoint1 ? _wayPoint2 : _wayPoint1;
        NextMove();
    }

    public void Explode()
    {
        if (!IsInfected)
            return;

        IsInfected = false;
        IsExpoding = true;

        // stop moving animations
        _movingTweener.Kill();
        GetComponent<BoxCollider>().enabled = false;

        foreach (var spore in Spores)
        {
            ThrowPods(spore.up.normalized, spore);
        }

        transform.DORotate(new Vector3(0, 0, transform.eulerAngles.z + 360), 0.6f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        transform.DOScale(Vector3.one * 0.01f, ThrowPodDuration * 1.05f).OnComplete(() => Destroy(gameObject));
    }

    private void ThrowPods(Vector3 normal, Transform obj)
    {
        obj.GetComponent<BoxCollider>().enabled = true;
        obj.SetParent(null);
        obj.DOMove(
            obj.transform.position + normal * ThrowPodHeight,
            ThrowPodDuration)
            .OnComplete(() => Destroy(obj.gameObject)).SetEase(Ease.Linear);
    }
}
