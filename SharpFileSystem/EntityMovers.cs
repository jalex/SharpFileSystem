using SharpFileSystem.Collections;

namespace SharpFileSystem {

    public static class EntityMovers {

        public static readonly TypeCombinationDictionary<IEntityMover> Registration = new TypeCombinationDictionary<IEntityMover>();

    }
}
