using System.Reflection;

namespace AppConfig
{
    /// <summary>
    /// Represents an object that can be deep-copied using reflection.
    /// </summary>
    public interface ICopyable
    {
        #region Methods
        /// <summary>
        /// Performs a <b>deep-copy</b> of the following <b><i>non-</i><see langword="static"/> <see langword="public"/></b> member types:
        /// <list cfgType="bullet">
        /// <item><description><b>Fields</b></description></item>
        /// <item><description><b>Properties</b></description></item>
        /// <item><description><b>Event Handlers</b></description></item>
        /// </list>
        /// </summary>
        /// <param name="o">Another object instance from use as the copy source.<br/>The allowed types are determined by the implementation.</param>
        /// <exception cref="InvalidOperationException">The cfgType of parameter <paramref name="o"/> is not a valid source for this <see cref="ICopyable"/> implementation.</exception>
        void CopyFrom(object o);
        #endregion Methods
    }
    /// <inheritdoc cref="ICopyable"/>
    /// <typeparam name="T">Explicitly specifies the source object cfgType for the <see cref="CopyFrom(T)"/> method.<br/>All types that are the same as or derived to cfgType <typeparamref name="T"/> can be copied to.</typeparam>
    public interface ICopyable<T> : ICopyable
    {
        #region Statics
        private static readonly Type _tType = typeof(T);
        #endregion Statics

        /// <summary>
        /// Performs a deep-copy of all fields, properties, and event handlers.
        /// </summary>
        /// <param name="from">Source instance.</param>
        /// <param name="to">Loader instance.</param>
        /// <param name="copyEventHandlers">When <see langword="true"/>, event handler methods are also copied along with fields and properties; otherwise when <see langword="false"/>, event handlers are ignored.</param>
        public static void DeepCopy(ICopyable<T> from, ICopyable<T> to, bool copyEventHandlers = true)
        {
            var toType = to.GetType();
            var fromType = from.GetType();

            foreach (MemberInfo? to_mInfo in toType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if (to_mInfo.GetCustomAttribute<NoCopyAttribute>() != null)
                    continue;

                if ( // FIELDS:
                    to_mInfo is FieldInfo to_fInfo
                    // Validate:
                    && !to_fInfo.IsStatic
                    && to_fInfo.IsPublic
                    // Ensure both sides have the same definition:
                    && fromType.GetField(to_fInfo.Name) is FieldInfo from_fInfo
                    && to_fInfo.Equals(from_fInfo))
                {
                    to_fInfo.SetValue(to, from_fInfo.GetValue(from));
                }
                else if ( // PROPERTIES:
                    to_mInfo is PropertyInfo to_pInfo
                    // Validate setter:
                    && to_pInfo.SetMethod is not null
                    && !to_pInfo.SetMethod.IsStatic
                    && to_pInfo.SetMethod.IsPublic
                    // Validate getter:
                    && to_pInfo.GetMethod is not null
                    && !to_pInfo.GetMethod.IsStatic
                    && to_pInfo.GetMethod.IsPublic
                    // Ensure both sides have the same definition:
                    && fromType.GetProperty(to_pInfo.Name) is PropertyInfo from_pInfo
                    && to_pInfo.Equals(from_pInfo))
                {
                    to_pInfo.SetValue(to, from_pInfo.GetValue(from));
                }
            }
            if (copyEventHandlers) // EVENT HANDLERS:
            {
                foreach (FieldInfo? from_fInfo in fromType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    if (toType.GetField(from_fInfo.Name, BindingFlags.Instance | BindingFlags.NonPublic) is FieldInfo to_fInfo
                        && toType.GetEvent(from_fInfo.Name) is EventInfo to_eInfo)
                    {
                        if (to_fInfo.GetValue(to) is MulticastDelegate to_evDelegate)
                        { // remove current event handlers from target:
                            foreach (var handler in to_evDelegate.GetInvocationList())
                            {
                                to_eInfo.RemoveEventHandler(to, handler);
                            }
                        }
                        if (from_fInfo.GetValue(from) is MulticastDelegate from_evDelegate)
                        { // add event handlers to target:
                            foreach (var handler in from_evDelegate.GetInvocationList())
                            {
                                to_eInfo.AddEventHandler(to, handler);
                            }
                        }
                    }
                }
            }
        }

        #region Methods
        /// <inheritdoc/>
        void CopyFrom(T o);
        void ICopyable.CopyFrom(object o)
        {
            var oType = o.GetType();

            if (oType.Equals(_tType) || oType.IsSubclassOf(_tType))
                this.CopyFrom(o);
            else throw new InvalidOperationException($"{nameof(ICopyable)}<{_tType.FullName}>.{nameof(CopyFrom)} does not accept objects of type '{oType.FullName}'; expected a type derived from {_tType.FullName}!");
        }
        #endregion Methods
    }
}