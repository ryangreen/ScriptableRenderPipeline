using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    public partial class DecalProjectorComponent : IVersionable<DecalProjectorComponent.Version>
    {
        enum Version
        {
            Initial,
            UseZProjectionAxis
        }

        static readonly MigrationDescription<Version, DecalProjectorComponent> k_Migration = MigrationDescription.New(
            MigrationStep.New(Version.UseZProjectionAxis, (DecalProjectorComponent decal) =>
            {
                // rotate so projection move from -Y to Z but childs keep same positions and rotations
                decal.transform.Rotate(new Vector3(90, 0, 0));
                foreach(Transform child in decal.transform)
                {
                    child.RotateAround(decal.transform.position, decal.transform.right, -90f);
                }
            })
        );

        [SerializeField]
        Version m_HDProbeVersion;
        Version IVersionable<Version>.version { get => m_HDProbeVersion; set => m_HDProbeVersion = value; }

        void Awake() => k_Migration.Migrate(this);
    }
}
