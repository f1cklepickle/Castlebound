using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using UnityEngine;

namespace Castlebound.Tests.Combat
{
    public class FeedbackEventChannelContractTests
    {
        private static Type FindTypeByName(string typeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (types == null)
                    continue;

                for (int i = 0; i < types.Length; i++)
                {
                    var t = types[i];
                    if (t != null && t.Name == typeName)
                        return t;
                }
            }

            return null;
        }

        [Test]
        public void FeedbackEventChannel_ContractExistsAndRaises()
        {
            var cueEnumType = FindTypeByName("FeedbackCueType");
            var cueType = FindTypeByName("FeedbackCue");
            var channelType = FindTypeByName("FeedbackEventChannel");

            Assert.NotNull(cueEnumType, "FeedbackCueType enum must exist for feedback cues.");
            Assert.NotNull(cueType, "FeedbackCue struct must exist to carry feedback data.");
            Assert.NotNull(channelType, "FeedbackEventChannel ScriptableObject must exist.");

            Assert.IsTrue(typeof(ScriptableObject).IsAssignableFrom(channelType),
                "FeedbackEventChannel should derive from ScriptableObject.");

            var eventInfo = channelType.GetEvent("Raised");
            Assert.NotNull(eventInfo, "FeedbackEventChannel should expose a Raised event.");

            var raiseMethod = channelType.GetMethod("Raise", new[] { cueType });
            Assert.NotNull(raiseMethod, "FeedbackEventChannel should expose Raise(FeedbackCue).");

            var channel = ScriptableObject.CreateInstance(channelType);

            var raisedFlag = new StrongBox<bool>(false);
            var parameter = Expression.Parameter(cueType, "cue");
            var setTrue = Expression.Assign(
                Expression.Field(Expression.Constant(raisedFlag), nameof(StrongBox<bool>.Value)),
                Expression.Constant(true));
            var handlerLambda = Expression.Lambda(eventInfo.EventHandlerType, setTrue, parameter);
            var handler = handlerLambda.Compile();
            eventInfo.AddEventHandler(channel, handler);

            var cue = Activator.CreateInstance(cueType);
            var enumValues = Enum.GetValues(cueEnumType);
            if (enumValues.Length > 0)
            {
                var typeField = cueType.GetField("Type");
                if (typeField != null)
                {
                    typeField.SetValue(cue, enumValues.GetValue(0));
                }
                else
                {
                    var typeProp = cueType.GetProperty("Type");
                    if (typeProp != null && typeProp.CanWrite)
                        typeProp.SetValue(cue, enumValues.GetValue(0));
                }
            }

            raiseMethod.Invoke(channel, new[] { cue });

            Assert.IsTrue(raisedFlag.Value, "FeedbackEventChannel.Raise should notify listeners via Raised event.");

            ScriptableObject.DestroyImmediate(channel);
        }
    }
}
