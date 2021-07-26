using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NUnit.Framework;

class DelegateTests {
    abstract class AbstractClass {
        public abstract int Foo();
    }

    class Child : AbstractClass {
        public override int Foo() {
            return 1;
        }
    }

    [Test]
    public static void Test() {
        var child = new Child();
        var act = new Func<int>(child.Foo);
        child = null;

        var regResult = child.Foo();
        var actResult = act.Invoke();

        Assert.AreEqual(regResult, actResult);
    }
}
