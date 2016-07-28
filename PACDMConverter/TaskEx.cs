using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace PACDMConverter
{
	/// <summary>Extensions related to the <see cref="Task"/> classes. Supports implementing "async"-style methods in C#4 using iterators.</summary>
	public static class TaskEx
	{
		/// <summary>
		/// Return a Completed <see cref="Task{TResult}"/> with a specific <see cref="Task{TResult}.Result"/> value.
		/// </summary>
		public static Task<TResult> FromResult<TResult>(TResult resultValue)
		{
			var completionSource = new TaskCompletionSource<TResult>();
			completionSource.SetResult(resultValue);
			return completionSource.Task;
		}


		/// <summary>Internal marker type for using <see cref="ToTask{T}"/> to implement <see cref="ToTask"/>.</summary>
		private abstract class VoidResult { }

		/// <summary>
		/// Transform an enumeration of <see cref="Task"/> into a single non-Result <see cref="Task"/>.
		/// </summary>
		public static Task ToTask(this IEnumerable<Task> tasks)
		{
			return ToTask<VoidResult>(tasks);
		}

		/// <summary>
		/// Transform an enumeration of <see cref="Task"/> into a single <see cref="Task{TResult}"/>.
		/// The final <see cref="Task"/> in <paramref name="tasks"/> must be a <see cref="Task{TResult}"/>.
		/// </summary>
		public static Task<TResult> ToTask<TResult>(this IEnumerable<Task> tasks)
		{
			var taskScheduler =
				SynchronizationContext.Current == null
					? TaskScheduler.Default : TaskScheduler.FromCurrentSynchronizationContext();
			var taskEnumerator = tasks.GetEnumerator();
			var completionSource = new TaskCompletionSource<TResult>();

			ToTaskDoOneStep(taskEnumerator, taskScheduler, completionSource, null);
			return completionSource.Task;
		}

		/// <summary>
		/// If the previous task Canceled or Faulted, complete the master task with the same <see cref="Task.Status"/>.
		/// Obtain the next <see cref="Task"/> from the <paramref name="taskEnumerator"/>.
		/// If none, complete the master task, possibly with the <see cref="Task{T}.Result"/> of the last task.
		/// Otherwise, set up the task with a continuation to come do this again when it completes.
		/// </summary>
		private static void ToTaskDoOneStep<TResult>(
			IEnumerator<Task> taskEnumerator, TaskScheduler taskScheduler,
			TaskCompletionSource<TResult> completionSource, Task completedTask)
		{
			try
			{
				// Check status of previous nested task (if any), and stop if Canceled or Faulted.
				// In these cases, we are abandoning the enumerator, so we must dispose it.
				TaskStatus status;
				if (completedTask == null)
				{
					// This is the first task from the iterator; skip status check.
				}
				else if ((status = completedTask.Status) == TaskStatus.Canceled)
				{
					taskEnumerator.Dispose();
					completionSource.SetCanceled();
					return;
				}
				else if (status == TaskStatus.Faulted)
				{
					taskEnumerator.Dispose();
					completionSource.SetException(completedTask.Exception.InnerExceptions);
					return;
				}
			}
			catch (Exception ex)
			{
				// Return exception from disposing the enumerator.
				completionSource.SetException(ex);
				return;
			}

			// Find the next Task in the iterator; handle cancellation and other exceptions.
			Boolean haveMore;
			try
			{
				// Enumerator disposes itself if it throws an exception or completes (returns false).
				haveMore = taskEnumerator.MoveNext();

			}
			catch (OperationCanceledException)
			{
				//if (cancExc.CancellationToken == cancellationToken) completionSource.SetCanceled();
				//else completionSource.SetException(cancExc);
				completionSource.SetCanceled();
				return;
			}
			catch (Exception exc)
			{
				completionSource.SetException(exc);
				return;
			}

			if (!haveMore)
			{
				// No more tasks; set the result from the last completed task (if any, unless no result is requested).
				// We know it's not Canceled or Faulted because we checked at the start of this method.
				if (typeof(TResult) == typeof(VoidResult))
				{        // No result
					completionSource.SetResult(default(TResult));

				}
				else if (!(completedTask is Task<TResult>))
				{     // Wrong result
					completionSource.SetException(new InvalidOperationException(
						"Asynchronous iterator " + taskEnumerator +
							" requires a final result task of type " + typeof(Task<TResult>).FullName +
							(completedTask == null ? ", but none was provided." :
								"; the actual task type was " + completedTask.GetType().FullName)));

				}
				else
				{
					completionSource.SetResult(((Task<TResult>)completedTask).Result);
				}

			}
			else
			{
				// When the nested task completes, continue by performing this function again.
				// Note: This is NOT a recursive call; the current method activation will complete
				// almost immediately and independently of the lambda continuation.
				taskEnumerator.Current.ContinueWith(
					nextTask => ToTaskDoOneStep(taskEnumerator, taskScheduler, completionSource, nextTask),
					taskScheduler);
			}
		}
	}
}
