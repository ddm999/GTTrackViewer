using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GTTrackEditor.ModelEntities
{
    public class TrackMeshModel : BaseModelEntity
    {
        public override bool CanRotate => false;

        public override bool CanTranslate => false;

        public override bool CanScale => false;
    }
}
