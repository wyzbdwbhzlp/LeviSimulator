using UnityEngine;

namespace PlayerControllers.CalculationPhysicsComponents
{
    public class SlidingPhysicsCalculationComponent:ICalculationPhysicsComponent
    {
        private PlayerMovementController _movementController;
        
    

        public void OnInit(PlayerMovementController movementController)
        {
            _movementController = movementController;
        }

        public bool CanSliding()
        {
            if (!_movementController.IsGrounded) return false;
           
            return true;
        }

 public void HandleMovementPhysics()
{
    if (_movementController == null || !_movementController.IsGrounded) return;

    var rb = _movementController.PlayerRigidbody;
    Vector3 groundNormal = _movementController.GroundNormal;
    var minSlopeAngle = _movementController.MinSlopeAngle;
    var downhillAccelScale = _movementController.DownhillAccelScale;
    var stickToGroundForce = _movementController.StickToGroundForce;

    // 当前水平速度
    Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    float horizontalSpeed = horizontalVel.magnitude;

    // 计算坡度角
    float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
    // 计算“下坡方向”
    Vector3 downhillDir = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

    // 判断是否在“顺着下坡”移动
    bool movingDownhill = downhillDir.sqrMagnitude > 0.01f && Vector3.Dot(horizontalVel.normalized, downhillDir) > 0.5f;

    // 1. 处理下坡加速
    if (movingDownhill && slopeAngle > minSlopeAngle)
    {
        float g = Physics.gravity.magnitude;
        float downhillAccel = g * Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * downhillAccelScale;
        rb.AddForce(downhillDir * downhillAccel, ForceMode.Acceleration);
    }
    // 2. 处理平地/上坡减速
    else
    {
        if (horizontalSpeed > 0.1f)
        {
            // 将 SlideDeceleration 作为摩擦系数来计算减速力
            // 这样 SlideDeceleration 就可以是一个正数，值越小，滑得越远
            float friction = _movementController.SlideDeceleration; 
            
            // 应用减速力，使用 Lerp 平滑过渡，避免速度突变
            Vector3 newVelocity = Vector3.Lerp(horizontalVel, Vector3.zero, friction * Time.deltaTime);
            rb.linearVelocity = new Vector3(newVelocity.x, rb.linearVelocity.y, newVelocity.z);
        }
    }

    // 3. 施加额外的贴地力，防止在坡上起飞
    if (slopeAngle > 1f)
    {
        rb.AddForce(Vector3.down * stickToGroundForce, ForceMode.Force);
    }

    // 4. 限制最大速度
    float baseSlideMax = _movementController.MaxHorizontalSpeed * _movementController.SlideSpeedMultiplier;
    float dynamicMax = baseSlideMax * (1f + Mathf.Clamp01(slopeAngle / 45f));
    
    Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    if (currentHorizontalVel.magnitude > dynamicMax)
    {
        Vector3 limitedVel = currentHorizontalVel.normalized * dynamicMax;
        rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
    }
}
    }
}