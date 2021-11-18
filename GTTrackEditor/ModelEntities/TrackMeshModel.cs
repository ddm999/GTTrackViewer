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
        public override bool CanRotateX => false;
        public override bool CanRotateY => false;
        public override bool CanRotateZ => false;

        public override void OnMove()
        {
            throw new NotImplementedException();
        }
    }
}
