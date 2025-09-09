// using UnityEngine;
//
// namespace PlayerControllers.CalculationPhysicsComponents
// {
//     /// <summary>
//     /// 处理空中（跳跃/下落）状态物理计算的组件。
//     /// 允许空中加速，并在无输入时保持惯性。
//     /// </summary>
//     public class AirbornePhysicsCalculationComponent : ICalculationPhysicsComponent
//     {
//         private PlayerMovementController _movementController;
//
//         public void OnInit(PlayerMovementController movementController)
//         {
//             _movementController = movementController;
//         }
//
//         public void HandleMovementPhysics()
//         {
//             if (_movementController == null || _movementController.IsGrounded)
//             {
//                 // 此组件仅在空中时工作
//                 return;
//             }
//
//             var rb = _movementController.PlayerRigidbody;
//             var tendency = _movementController.FixedPlayerMovementTendencyByPlayerLookAt;
//             var airAcceleration = _movementController.AirAcceleration;
//             var maxAirSpeed = _movementController.MaxAirSpeed;
//
//             // 如果有移动输入，则施加空中加速度
//             if (tendency.magnitude > 0.1f)
//             {
//                 Vector3 desiredForce = tendency.normalized * airAcceleration;
//                 rb.AddForce(desiredForce, ForceMode.Acceleration);
//             }
//
//             // 限制空中最大水平速度
//             Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
//             if (horizontalVel.magnitude > maxAirSpeed)
//             {
//                 Vector3 limitedVel = horizontalVel.normalized * maxAirSpeed;
//                 rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
//             }
//         }
//     }
// }