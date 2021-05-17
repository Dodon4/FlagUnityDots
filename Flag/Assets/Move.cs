
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

public class Move : SystemBase
{
    EntityQuery query;
    protected override void OnCreate()
    {
        query = GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<MSpeed>(), ComponentType.ReadOnly<WaveData>(), ComponentType.ReadOnly<zOffset>(), ComponentType.ReadOnly<TimeComp>());
    }

    [BurstCompile]
    private struct CalcWaveJob : IJobEntityBatch
    {
        public float elapsedTime;
        public ComponentTypeHandle<Translation> transType;
        [ReadOnly] public ComponentTypeHandle<MSpeed> moveSpeedType;
        [ReadOnly] public ComponentTypeHandle<WaveData> waveDataType;
        [ReadOnly] public ComponentTypeHandle<zOffset> zStartType;
        [ReadOnly] public ComponentTypeHandle<TimeComp> timeComp;

        public void Execute(ArchetypeChunk chunk, int chunkIndex)
        {
            var Chunktrans = chunk.GetNativeArray(transType);
            var ChunkmoveSpeed = chunk.GetNativeArray(moveSpeedType);
            var ChunkwaveData = chunk.GetNativeArray(waveDataType);
            var ChunkzStart = chunk.GetNativeArray(zStartType);
            var Chunktime = chunk.GetNativeArray(timeComp);
            for (var i = 0; i < chunk.Count; i++)
            {
                var moveSpeed = ChunkmoveSpeed[i];
                if (moveSpeed.isMoving)
                {
                    var trans = Chunktrans[i];
                    var waveData = ChunkwaveData[i];
                    var zStart = ChunkzStart[i];
                    var time = Chunktime[i];
                    Chunktrans[i] = new Translation
                    {
                        Value = new float3(trans.Value.x, trans.Value.y,
                        zStart.Offset + waveData.amplitude * math.sin((elapsedTime - time.Value) * moveSpeed.Value
                        + trans.Value.x * waveData.xOffset + trans.Value.y * waveData.yOffset))
                    };
                }
            }
        }
    }
    protected override void OnUpdate()
    {
        var Chunktrans = this.GetComponentTypeHandle<Translation>();
        var ChunkmoveSpeed = this.GetComponentTypeHandle<MSpeed>(true);
        var ChunkwaveData = this.GetComponentTypeHandle<WaveData>(true);
        var ChunkzStart = this.GetComponentTypeHandle<zOffset>(true);
        var Chunktime = this.GetComponentTypeHandle<TimeComp>(true);
        var job = new CalcWaveJob()
        {
            transType = Chunktrans,
            moveSpeedType = ChunkmoveSpeed,
            waveDataType = ChunkwaveData,
            zStartType = ChunkzStart,
            timeComp = Chunktime,
            elapsedTime = (float)Time.ElapsedTime
        };
        Dependency = job.Schedule(query, Dependency);

    }
}