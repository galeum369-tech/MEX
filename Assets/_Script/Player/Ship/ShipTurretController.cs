using UnityEngine;

public class ShipTurretController
{
    Transform turretTransform;

    public ShipTurretController(Transform turretTransform)
    {
        this.turretTransform = turretTransform;
    }

    public void LookAt(Vector2 targetPosition)
    {
        if(turretTransform == null) return;

        // 포탑에서 마우스까지의 방향 벡터 계산
        Vector2 direction = targetPosition - (Vector2)turretTransform.position;

        //각도계산
        //포탑도 위를 보고 있다 가정 -90도
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        //회전
        //본체가 돌아도 포탑이 마우스를 향하게 하기 위해 월드 좌표계 사용
        turretTransform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
