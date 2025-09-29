namespace Flint.Common
{
	abstract class Disposable : IDisposable
	{
		private bool _disposed = false;

		protected abstract void BaseDispose(bool disposing);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		~Disposable()
		{
			Dispose(false);
		}

		private void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			BaseDispose(disposing);
			_disposed = true;
		}
	}
}
