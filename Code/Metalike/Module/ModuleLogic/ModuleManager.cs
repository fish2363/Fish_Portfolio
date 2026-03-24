using Core.EventBus;
using GondrLib.Dependencies;
using System.Collections.Generic;

namespace ManagingSystem
{
    public class ModuleManager : BaseManager<ModuleManager>
    {
        [Inject] private CharacterManager _manager;

        private Dictionary<CharacterSO, ModuleContainer> _characterDict = new();


        public override void StartManager()
        {
            ChangePartyHandle(new ChangeEvent(_manager.CurrentParty));
        }

        public override void Subscribe()
        {
            Bus<SelectedEvent>.OnEvent += SwichModuleHandle;
            Bus<ChangeEvent>.OnEvent += ChangePartyHandle;


            Bus<GetModuleEvents>.OnEvent += AddModuleHandle;
            Bus<ModuleSelectEvent>.OnEvent += GetModuleSelectInfoHandle;
        }

        public override void Unsubscribe()
        {
            Bus<SelectedEvent>.OnEvent -= SwichModuleHandle;
            Bus<ChangeEvent>.OnEvent -= ChangePartyHandle;

            Bus<GetModuleEvents>.OnEvent -= AddModuleHandle;
        }

        private void GetModuleSelectInfoHandle(ModuleSelectEvent evt)
        {
            Bus<ModuleSelectUIChangeInfoEvent>.Raise(new ModuleSelectUIChangeInfoEvent(_characterDict[evt.character]));
        }

        private void AddModuleHandle(GetModuleEvents evt)
        {
            _characterDict[evt.owner].ActivePassives.Add(evt.passive);
        }

        private void ChangePartyHandle(ChangeEvent evt)
        {
            var oldKeys = new List<CharacterSO>(_characterDict.Keys);

            foreach (var ch in oldKeys)
            {
                if (!evt.infos.Contains(ch))
                {
                    _characterDict.Remove(ch);
                }
            }

            foreach (var ch in evt.infos)
            {
                if (!_characterDict.ContainsKey(ch))
                {
                    _characterDict.Add(ch, new ModuleContainer());
                }
            }
        }

        private void SwichModuleHandle(SelectedEvent evt)
        {
            Bus<PassiveChangeEvent>.Raise(new PassiveChangeEvent(_characterDict[evt.info]));
        }
    }
}
