using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeleporterController : MonoBehaviour
{
    [SerializeField] private float overlapCircleRadius;
    [SerializeField] private TeleporterController connectedTeleporter;

    private List<Collider2D> previousOverlaps = new List<Collider2D>();

    private void Update()
    {
        Collider2D[] overlapColliders = Physics2D.OverlapCircleAll(transform.position, overlapCircleRadius);

        foreach (Collider2D previousCollider in previousOverlaps)
        {
            if (overlapColliders.Contains(previousCollider)) continue;

            if (previousCollider.TryGetComponent<ITeleportable>(out var colliderComp))
            {
                if (colliderComp.ShadowReference.activeSelf == true) colliderComp.ShadowReference.SetActive(false);
            }
        }

        previousOverlaps.Clear();

        foreach (Collider2D collider in overlapColliders)
        {
            previousOverlaps.Add(collider);

            if (collider.TryGetComponent<ITeleportable>(out var teleportableComp))
            {
                if (!teleportableComp.ShadowReference.activeSelf) teleportableComp.ShadowReference.SetActive(true);

                Vector3 shadowReferenceDisplacement = collider.transform.position - transform.position;
                teleportableComp.ShadowReference.transform.position = connectedTeleporter.transform.position + shadowReferenceDisplacement;

                if (collider.TryGetComponent<PacStudentController>(out var studentController))
                {
                    Animator shadowAnimator = teleportableComp.ShadowReference.GetComponent<Animator>();
                    shadowAnimator.SetTrigger(studentController.GetCurrentDirectionAnimation());
                    shadowAnimator.speed = 1.0f;
                }

                if (Vector2.Distance((Vector2)transform.position, (Vector2)collider.transform.position) < 0.025f)
                {
                    if (collider.transform.position.x > transform.position.x && connectedTeleporter.transform.position.x > transform.position.x) TeleportationHandle(collider);
                    if (collider.transform.position.x < transform.position.x && connectedTeleporter.transform.position.x < transform.position.x) TeleportationHandle(collider);
                }
            }
        }
    }

    private void TeleportationHandle(Collider2D collider)
    {
        if (collider.TryGetComponent<PacStudentController>(out var studentController))
        {
            if (studentController.IsInTeleport) return;
            studentController.IsInTeleport = true;

            Vector2 startPosNormalized = studentController.GetWorldPosNormalized(new Vector2(
                connectedTeleporter.transform.position.x + 0.5f * Mathf.Sign(studentController.transform.position.x - transform.position.x), 
                connectedTeleporter.transform.position.y
            ));
            Vector2 endPosNormalized = studentController.GetWorldPosNormalized(new Vector2(
                connectedTeleporter.transform.position.x - 0.5f * Mathf.Sign(studentController.transform.position.x - transform.position.x),
                connectedTeleporter.transform.position.y
            ));

            studentController.TeleportMoveStudent(startPosNormalized, endPosNormalized);
        }
    }
}
