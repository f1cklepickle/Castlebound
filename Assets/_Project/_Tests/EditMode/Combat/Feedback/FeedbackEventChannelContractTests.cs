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

        [Test]
        public void Raise_TargetedCue_NotifiesOnlyMatchingTargetListener()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            int matchingCalls = 0;
            int otherCalls = 0;
            Action<FeedbackCue> matchingListener = _ => matchingCalls++;
            Action<FeedbackCue> otherListener = _ => otherCalls++;

            channel.SubscribeTarget(10, matchingListener);
            channel.SubscribeTarget(20, otherListener);
            channel.Raise(new FeedbackCue(FeedbackCueType.PlayerHitEnemy, Vector3.zero, 10));

            Assert.That(matchingCalls, Is.EqualTo(1));
            Assert.That(otherCalls, Is.Zero);
            ScriptableObject.DestroyImmediate(channel);
        }

        [Test]
        public void UnsubscribeTarget_RemovesTargetedListener()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            int calls = 0;
            Action<FeedbackCue> listener = _ => calls++;

            channel.SubscribeTarget(10, listener);
            channel.UnsubscribeTarget(10, listener);
            channel.Raise(new FeedbackCue(FeedbackCueType.PlayerHitEnemy, Vector3.zero, 10));

            Assert.That(calls, Is.Zero);
            ScriptableObject.DestroyImmediate(channel);
        }

        [Test]
        public void Raise_UntargetedCue_StillNotifiesGlobalListener()
        {
            var channel = ScriptableObject.CreateInstance<FeedbackEventChannel>();
            int calls = 0;
            channel.Raised += _ => calls++;

            channel.Raise(new FeedbackCue(FeedbackCueType.PlayerHit, Vector3.zero));

            Assert.That(calls, Is.EqualTo(1));
            ScriptableObject.DestroyImmediate(channel);
        }
    }
}
