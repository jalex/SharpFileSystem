using SharpFileSystem.Collections;

namespace SharpFileSystem {

    public static class EntityCopiers {

        public static readonly TypeCombinationDictionary<IEntityCopier> Registration = new TypeCombinationDictionary<IEntityCopier>();

    }
}
