using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Blade.Players.States
{
    public class PlayerMoveState : PlayerCanAttackState
    {
        private readonly string _footStepEffectName = "FootStep";
        private EntityAnimator _animator;
        private EntityStatCompo _statCompo;

        private readonly int _speedHash = Animator.StringToHash("Speed");
        private float _currentAnimSpeed = 0f;
        private float _targetAnimSpeed = 0f;

        public PlayerMoveState(Entity entity, int animationHash) : base(entity, animationHash)
        {
            _animator = entity.GetCompo<EntityAnimator>();
            _statCompo = entity.GetCompo<EntityStatCompo>();
        }

        public override void Enter()
        {
            base.Enter();
            _player.PlayerInput.OnRunningPressed += HandleSpeedChange;

            _targetAnimSpeed = 1.0f;
            _currentAnimSpeed = 1.0f;
        }

        private void HandleSpeedChange(bool isRunning)
        {
            _targetAnimSpeed = isRunning ? 2.0f : 1.0f;

        }

        public override void Exit()
        {
            base.Exit();
            _player.PlayerInput.OnRunningPressed -= HandleSpeedChange;

            _animator.SetParam(_speedHash, 0f);
        }

        public override void Update()
        {
            base.Update();

            if (!_player.IsLocalPlayer) return;

            Vector2 movementKey = _player.PlayerInput.MovementKey;
            _movement.SetMovementDirection(movementKey);

            _currentAnimSpeed = Mathf.Lerp(_currentAnimSpeed, _targetAnimSpeed, Time.deltaTime * 10f);
            _animator.SetParam(_speedHash, _currentAnimSpeed);

            if (movementKey.magnitude < _inputThreshold)
                _player.ChangeState("IDLE");
        }

    }
}