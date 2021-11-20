using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTTrackEditor.ModelEntities
{
    public class TrackMeshModel : BaseModelEntity
    {
        public override bool CanTranslateX => false;
        public override bool CanTranslateY => false;
        public override bool CanTranslateZ => false;
        public override bool PitchRotationAllowed => false;
        public override bool YawRotationAllowed => false;
        public override bool RollRotationAllowed => false;

        public override void OnManipulation()
        {
            
        }

        public override void UpdateValues()
        {
            throw new NotImplementedException();
        }
    }
}
