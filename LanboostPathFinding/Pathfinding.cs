using System;
using System.Collections.Generic;
using System.Text;


namespace Lanboost.PathFinding
{

	public interface IPathFinder<N, E>
	{
		String FindPath(N n1, N n2);
		List<E> GetPath();
	}

	public interface IPathFinderWithErrorState<N, E>
	{
		bool FindPath(N n1, N n2);
		List<E> GetPath();
		bool LastFindPathWasSuccessful();
		String LastFindPathError();
	}

	public class PathFinderWithErrorStateDecorator<N, E> : IPathFinderWithErrorState<N, E>
	{
		IPathFinder<N, E> decorator;

		public PathFinderWithErrorStateDecorator(IPathFinder<N, E> decorator)
		{
			this.decorator = decorator;
		}

		protected String LastErrorMessageValue
		{
			get; set;
		}

		public bool FindPath(N n1, N n2)
		{
			var error = decorator.FindPath(n1, n2);

			LastErrorMessageValue = error;
			return LastFindPathWasSuccessful();
		}

		public List<E> GetPath()
		{
			return decorator.GetPath();
		}

		public string LastFindPathError()
		{
			return LastErrorMessageValue;
		}

		public bool LastFindPathWasSuccessful()
		{
			return LastErrorMessageValue == null;
		}
	}
}