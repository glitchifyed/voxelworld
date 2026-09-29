using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Godot;

public class ThreadedTasks<T>
{
    private readonly ConcurrentQueue<T> _tasks = [];
    private readonly Action<T> _action;
    
    
    public ThreadedTasks(int threads, Action<T> action)
    {
        _action = action;
        
        for (int i = 0; i < threads; i++)
            Task.Run(DequeueTasks);
    }
    
    
    private void DequeueTasks()
    {
        while (true)
        {
            if (!_tasks.TryDequeue(out T task))
                continue;
            
            _action(task);
        }
    }
    
    public void AddTask(T task)
        => _tasks.Enqueue(task);
}