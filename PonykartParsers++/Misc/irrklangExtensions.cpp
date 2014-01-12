#include <LinearMath/btVector3.h>
#include <OgreVector3.h>
#include "Misc/irrklangExtensions.h"

using namespace irrklang;
using namespace Ogre;

namespace Extensions
{
	vec3df toSoundVector(const Vector3& vec)
	{
		return vec3df(vec.x, vec.y, vec.z);
	}

	vec3df toSoundVector(const btVector3& vec)
	{
		return vec3df(vec.x(), vec.y(), vec.z());
	}
} // Extensions
