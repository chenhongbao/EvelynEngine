/*
Copyright (C) 2022  Chen Hongbao<chenhongbao@outlook.com>

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as
published by the Free Software Foundation, either version 3 of the
License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace Evelyn.Model
{
    public struct TriggerCondition
    {
        private TriggerType? _triggerType = null;
        private InstrumentState? _stateChange = null;
        private DateTime? _moment = null;

        public TriggerCondition()
        {
        }

        public TriggerType When
        {
            get => _triggerType ?? throw new NoValueException("Trigger type has no value.");
            set => _triggerType = value;
        }

        public InstrumentState StateChange
        {
            get => _stateChange ?? throw new NoValueException("Trigger instrument state has no value.");
            set => _stateChange = value;
        }

        public DateTime Time
        {
            get => _moment ?? throw new NoValueException("Trigger moment has no value.");
            set => _moment = value;
        }
    }
}