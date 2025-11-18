using System.Collections.Generic;
using UnityEngine;

namespace WattsTap.Core.GameLoop
{
    public class UpdateService : MonoBehaviour, IUpdateService
    {
        private static readonly List<IUpdatable> _updatables = new();
        private static readonly List<IUpdatable> _pendingAdd = new();
        private static readonly List<IUpdatable> _pendingRemove = new();

        private bool _isUpdating;

        public int InitializationOrder => 2;
        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            if (IsInitialized)
            {
                return;
            }

            DontDestroyOnLoad(gameObject);
            IsInitialized = true;
        }

        public void Shutdown()
        {
            if (!IsInitialized)
            {
                return;
            }

            IsInitialized = false;
            _updatables.Clear();
            _pendingAdd.Clear();
            _pendingRemove.Clear();
            Destroy(gameObject);
        }

        public void Register(IUpdatable updatable)
        {
            if (updatable == null)
            {
                return;
            }

            if (_isUpdating)
            {
                if (!_updatables.Contains(updatable) && !_pendingAdd.Contains(updatable))
                {
                    _pendingAdd.Add(updatable);
                }
            }
            else
            {
                if (!_updatables.Contains(updatable))
                {
                    _updatables.Add(updatable);
                }
            }
        }

        public void Unregister(IUpdatable updatable)
        {
            if (updatable == null)
            {
                return;
            }
            
            if (_isUpdating)
            {
                if (_updatables.Contains(updatable) && !_pendingRemove.Contains(updatable))
                {
                    _pendingRemove.Add(updatable);
                }

                // Also remove from pending add if it was queued this frame
                _pendingAdd.Remove(updatable);
            }
            else
            {
                _updatables.Remove(updatable);
            }
        }

        private void FlushQueues()
        {
            if (_pendingRemove.Count > 0)
            {
                for (int i = 0; i < _pendingRemove.Count; i++)
                {
                    _updatables.Remove(_pendingRemove[i]);
                }

                _pendingRemove.Clear();
            }

            if (_pendingAdd.Count > 0)
            {
                for (int i = 0; i < _pendingAdd.Count; i++)
                {
                    var u = _pendingAdd[i];
                    if (!_updatables.Contains(u))
                    {
                        _updatables.Add(u);
                    }
                }

                _pendingAdd.Clear();
            }
        }

        public void Update()
        {
            FlushQueues(); // Apply queued changes from previous frame
            _isUpdating = true;
            for (int i = 0; i < _updatables.Count; i++)
            {
                _updatables[i].OnUpdate();
            }

            _isUpdating = false;
            FlushQueues(); // Apply changes requested during this Update
        }
    }
}