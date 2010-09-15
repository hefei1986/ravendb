﻿using System;
using System.Threading;

namespace Raven.Client.Client.Async
{
	/// <summary>
	/// An <see cref="IAsyncResult"/> that wraps another <see cref="IAsyncResult"/>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class WrapperAsyncData<T> : IAsyncResult
	{
		private readonly IAsyncResult inner;
		private readonly T wrapped;

		/// <summary>
		/// Initializes a new instance of the <see cref="WrapperAsyncData&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="inner">The inner.</param>
		/// <param name="wrapped">The wrapped.</param>
		public WrapperAsyncData(IAsyncResult inner, T wrapped)
		{
			this.inner = inner;
			this.wrapped = wrapped;
		}

		public bool IsCompleted
		{
			get { return inner.IsCompleted; }
		}

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				return inner.AsyncWaitHandle;
			}
		}

		public object AsyncState
		{
			get { return inner.AsyncState; }
		}

		public bool CompletedSynchronously
		{
			get { return inner.CompletedSynchronously; }
		}

		/// <summary>
		/// Gets the wrapped instance.
		/// </summary>
		/// <value>The wrapped.</value>
		public T Wrapped
		{
			get { return wrapped; }
		}

		/// <summary>
		/// Gets the inner <see cref="IAsyncResult"/>.
		/// </summary>
		/// <value>The inner.</value>
		public IAsyncResult Inner
		{
			get {
				return inner;
			}
		}
	}
}