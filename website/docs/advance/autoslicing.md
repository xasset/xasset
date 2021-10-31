# 自动分帧

自动分帧技术主要用来保障程序运行时的 FPS。

自动分帧的原理是根据当前可用的单帧时间，有选择地执行更新业务，如果当前帧可用单帧时间为 <= 0，则把没处理完的操作放到下一帧处理，从而避免单帧内大量业务堆积导致程序运行的卡顿问题。

在 xasset 7.0 中，程序的所有更新操作都交给 Updater 的组件类集中处理。可以通过 Updater 的以下属性预定义最大单帧更新时间片：

- Updater.maxUpdateTimeSlice

Updater 组件在帧开始时会记录当前帧的起始时间，这样，每次执行更新的时候，可以比较当前时间和起始时间的时间差来获取当前帧的剩余可用时间，剩余时间 > 0 就可以继续更新，反之则应该中断当前帧的处理逻辑，以下是使用自动分帧异步删除历史文件的代码示例:

```csharp
protected override void Update()
{
    if (status == OperationStatus.Processing)
    {
        while (files.Count > 0)
        {
            progress = (count - files.Count) * 1f / count;
            var file = files[0];
            if (File.Exists(file)) File.Delete(file);
            files.RemoveAt(0);
            // busy 状态返回为 true 时，表示没有剩余时间了
            if (Updater.Instance.busy) break;
        }

        Finish();
    }
}
```
