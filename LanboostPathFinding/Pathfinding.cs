using System;
using System.Collections.Generic;
using System.Text;


namespace Lanboost.PathFinding
{

	public interface IPathFinder<N, E>
	{
		String FindPath(N n1, N n2);
		List<E> GetPathLinks();
		List<N> GetPath();
	}
}