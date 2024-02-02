"use strict";

var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; };

/**
 * @license
 * Lodash (Custom Build) lodash.com/license | Underscore.js 1.8.3 underscorejs.org/LICENSE
 * Build: `lodash strict -p -o src/vendors/lodash.ptjs.min.js include="capitalize,chain,concat,debounce,filter,find,findIndex,groupBy,keys,isObject,orderBy,map,mapValues,pickBy,replace,uniqueId"`
 */
;(function () {
  "use strict";
  function t(t, n) {
    return t.set(n[0], n[1]), t;
  }function n(t, n) {
    return t.add(n), t;
  }function r(t, n, r) {
    switch (r.length) {case 0:
        return t.call(n);case 1:
        return t.call(n, r[0]);case 2:
        return t.call(n, r[0], r[1]);case 3:
        return t.call(n, r[0], r[1], r[2]);}return t.apply(n, r);
  }function e(t, n, r, e) {
    for (var u = -1, i = null == t ? 0 : t.length; ++u < i;) {
      var o = t[u];n(e, o, r(o), t);
    }return e;
  }function u(t, n) {
    for (var r = -1, e = null == t ? 0 : t.length; ++r < e && n(t[r], r, t) !== false;) {}return t;
  }function i(t, n) {
    for (var r = -1, e = null == t ? 0 : t.length, u = 0, i = []; ++r < e;) {
      var o = t[r];n(o, r, t) && (i[u++] = o);
    }return i;
  }function o(t, n) {
    return !!(null == t ? 0 : t.length) && h(t, n, 0) > -1;
  }function a(t, n) {
    for (var r = -1, e = null == t ? 0 : t.length, u = Array(e); ++r < e;) {
      u[r] = n(t[r], r, t);
    }return u;
  }function c(t, n) {
    for (var r = -1, e = n.length, u = t.length; ++r < e;) {
      t[u + r] = n[r];
    }return t;
  }function f(t, n, r, e) {
    var u = -1,
        i = null == t ? 0 : t.length;for (e && i && (r = t[++u]); ++u < i;) {
      r = n(r, t[u], u, t);
    }return r;
  }function s(t, n) {
    for (var r = -1, e = null == t ? 0 : t.length; ++r < e;) {
      if (n(t[r], r, t)) return true;
    }return false;
  }function _(t) {
    return t.split("");
  }function l(t, n, r, e) {
    for (var u = t.length, i = r + (e ? 1 : -1); e ? i-- : ++i < u;) {
      if (n(t[i], i, t)) return i;
    }return -1;
  }function h(t, n, r) {
    return n === n ? $(t, n, r) : l(t, p, r);
  }function p(t) {
    return t !== t;
  }function v(t) {
    return function (n) {
      return null == n ? ce : n[t];
    };
  }function y(t, n) {
    var r = t.length;for (t.sort(n); r--;) {
      t[r] = t[r].c;
    }return t;
  }function g(t, n) {
    for (var r = -1, e = Array(t); ++r < t;) {
      e[r] = n(r);
    }return e;
  }function d(t) {
    return function (n) {
      return t(n);
    };
  }function b(t, n) {
    return a(n, function (n) {
      return t[n];
    });
  }function w(t, n) {
    return t.has(n);
  }function j(t, n) {
    for (var r = t.length, e = 0; r--;) {
      t[r] === n && ++e;
    }return e;
  }function m(t, n) {
    return null == t ? ce : t[n];
  }function A(t) {
    return ti.test(t);
  }function O(t) {
    for (var n, r = []; !(n = t.next()).done;) {
      r.push(n.value);
    }return r;
  }function k(t) {
    var n = -1,
        r = Array(t.size);return t.forEach(function (t, e) {
      r[++n] = [e, t];
    }), r;
  }function x(t, n) {
    return function (r) {
      return t(n(r));
    };
  }function z(t, n) {
    for (var r = -1, e = t.length, u = 0, i = []; ++r < e;) {
      var o = t[r];o !== n && o !== pe || (t[r] = pe, i[u++] = r);
    }return i;
  }function S(t) {
    var n = -1,
        r = Array(t.size);return t.forEach(function (t) {
      r[++n] = t;
    }), r;
  }function $(t, n, r) {
    for (var e = r - 1, u = t.length; ++e < u;) {
      if (t[e] === n) return e;
    }return -1;
  }function R(t) {
    return A(t) ? E(t) : _(t);
  }function E(t) {
    return t.match(Zu) || [];
  }function I(t) {
    if (Fr(t) && !jo(t) && !(t instanceof L)) {
      if (t instanceof C) return t;if (di.call(t, "__wrapped__")) return fr(t);
    }return new C(t);
  }function T() {}function C(t, n) {
    this.__wrapped__ = t, this.__actions__ = [], this.__chain__ = !!n, this.__index__ = 0, this.__values__ = ce;
  }function L(t) {
    this.__wrapped__ = t, this.__actions__ = [], this.__dir__ = 1, this.__filtered__ = false, this.__iteratees__ = [], this.__takeCount__ = Ue, this.__views__ = [];
  }function F() {
    var t = new L(this.__wrapped__);return t.__actions__ = _n(this.__actions__), t.__dir__ = this.__dir__, t.__filtered__ = this.__filtered__, t.__iteratees__ = _n(this.__iteratees__), t.__takeCount__ = this.__takeCount__, t.__views__ = _n(this.__views__), t;
  }function B() {
    if (this.__filtered__) {
      var t = new L(this);t.__dir__ = -1, t.__filtered__ = true;
    } else t = this.clone(), t.__dir__ *= -1;return t;
  }function P() {
    var t = this.__wrapped__.value(),
        n = this.__dir__,
        r = jo(t),
        e = n < 0,
        u = r ? t.length : 0,
        i = Fn(0, u, this.__views__),
        o = i.start,
        a = i.end,
        c = a - o,
        f = e ? a : o - 1,
        s = this.__iteratees__,
        _ = s.length,
        l = 0,
        h = Ui(c, this.__takeCount__);
    if (!r || !e && u == c && h == c) return Ht(t, this.__actions__);var p = [];t: for (; c-- && l < h;) {
      f += n;for (var v = -1, y = t[f]; ++v < _;) {
        var g = s[v],
            d = g.iteratee,
            b = g.type,
            w = d(y);if (b == Te) y = w;else if (!w) {
          if (b == Ie) continue t;break t;
        }
      }p[l++] = y;
    }return p;
  }function U(t) {
    var n = -1,
        r = null == t ? 0 : t.length;for (this.clear(); ++n < r;) {
      var e = t[n];this.set(e[0], e[1]);
    }
  }function W() {
    this.__data__ = Ji ? Ji(null) : {}, this.size = 0;
  }function M(t) {
    var n = this.has(t) && delete this.__data__[t];return this.size -= n ? 1 : 0, n;
  }function N(t) {
    var n = this.__data__;if (Ji) {
      var r = n[t];return r === le ? ce : r;
    }return di.call(n, t) ? n[t] : ce;
  }function D(t) {
    var n = this.__data__;return Ji ? n[t] !== ce : di.call(n, t);
  }function V(t, n) {
    var r = this.__data__;return this.size += this.has(t) ? 0 : 1, r[t] = Ji && n === ce ? le : n, this;
  }function q(t) {
    var n = -1,
        r = null == t ? 0 : t.length;for (this.clear(); ++n < r;) {
      var e = t[n];this.set(e[0], e[1]);
    }
  }function G() {
    this.__data__ = [], this.size = 0;
  }function J(t) {
    var n = this.__data__,
        r = pt(n, t);return !(r < 0) && (r == n.length - 1 ? n.pop() : Ri.call(n, r, 1), --this.size, true);
  }function K(t) {
    var n = this.__data__,
        r = pt(n, t);
    return r < 0 ? ce : n[r][1];
  }function H(t) {
    return pt(this.__data__, t) > -1;
  }function Q(t, n) {
    var r = this.__data__,
        e = pt(r, t);return e < 0 ? (++this.size, r.push([t, n])) : r[e][1] = n, this;
  }function X(t) {
    var n = -1,
        r = null == t ? 0 : t.length;for (this.clear(); ++n < r;) {
      var e = t[n];this.set(e[0], e[1]);
    }
  }function Y() {
    this.size = 0, this.__data__ = { hash: new U(), map: new (Di || q)(), string: new U() };
  }function Z(t) {
    var n = In(this, t).delete(t);return this.size -= n ? 1 : 0, n;
  }function tt(t) {
    return In(this, t).get(t);
  }function nt(t) {
    return In(this, t).has(t);
  }function rt(t, n) {
    var r = In(this, t),
        e = r.size;return r.set(t, n), this.size += r.size == e ? 0 : 1, this;
  }function et(t) {
    var n = -1,
        r = null == t ? 0 : t.length;for (this.__data__ = new X(); ++n < r;) {
      this.add(t[n]);
    }
  }function ut(t) {
    return this.__data__.set(t, le), this;
  }function it(t) {
    return this.__data__.has(t);
  }function ot(t) {
    this.size = (this.__data__ = new q(t)).size;
  }function at() {
    this.__data__ = new q(), this.size = 0;
  }function ct(t) {
    var n = this.__data__,
        r = n.delete(t);return this.size = n.size, r;
  }function ft(t) {
    return this.__data__.get(t);
  }function st(t) {
    return this.__data__.has(t);
  }function _t(t, n) {
    var r = this.__data__;if (r instanceof q) {
      var e = r.__data__;if (!Di || e.length < se - 1) return e.push([t, n]), this.size = ++r.size, this;r = this.__data__ = new X(e);
    }return r.set(t, n), this.size = r.size, this;
  }function lt(t, n) {
    var r = jo(t),
        e = !r && wo(t),
        u = !r && !e && mo(t),
        i = !r && !e && !u && Ao(t),
        o = r || e || u || i,
        a = o ? g(t.length, String) : [],
        c = a.length;for (var f in t) {
      !n && !di.call(t, f) || o && ("length" == f || u && ("offset" == f || "parent" == f) || i && ("buffer" == f || "byteLength" == f || "byteOffset" == f) || Vn(f, c)) || a.push(f);
    }return a;
  }function ht(t, n, r) {
    var e = t[n];di.call(t, n) && Er(e, r) && (r !== ce || n in t) || dt(t, n, r);
  }function pt(t, n) {
    for (var r = t.length; r--;) {
      if (Er(t[r][0], n)) return r;
    }return -1;
  }function vt(t, n, r, e) {
    return io(t, function (t, u, i) {
      n(e, t, r(t), i);
    }), e;
  }function yt(t, n) {
    return t && ln(n, Gr(n), t);
  }function gt(t, n) {
    return t && ln(n, Jr(n), t);
  }function dt(t, n, r) {
    "__proto__" == n && Ci ? Ci(t, n, { configurable: true, enumerable: true, value: r, writable: true }) : t[n] = r;
  }function bt(t, n) {
    for (var r = -1, e = n.length, u = Array(e), i = null == t; ++r < e;) {
      u[r] = i ? ce : Vr(t, n[r]);
    }return u;
  }function wt(t, n, r, e, i, o) {
    var a,
        c = n & ve,
        f = n & ye,
        s = n & ge;if (r && (a = i ? r(t, e, i, o) : r(t)), a !== ce) return a;if (!Lr(t)) return t;var _ = jo(t);if (_) {
      if (a = Un(t), !c) return _n(t, a);
    } else {
      var l = lo(t),
          h = l == Je || l == Ke;if (mo(t)) return Yt(t, c);if (l == Ye || l == Me || h && !i) {
        if (a = f || h ? {} : Wn(t), !c) return f ? pn(t, gt(a, t)) : hn(t, yt(a, t));
      } else {
        if (!ri[l]) return i ? t : {};a = Mn(t, l, wt, c);
      }
    }o || (o = new ot());var p = o.get(t);if (p) return p;o.set(t, a);var v = s ? f ? Sn : zn : f ? Jr : Gr,
        y = _ ? ce : v(t);return u(y || t, function (e, u) {
      y && (u = e, e = t[u]), ht(a, u, wt(e, n, r, u, t, o));
    }), a;
  }function jt(t, n) {
    var r = [];
    return io(t, function (t, e, u) {
      n(t, e, u) && r.push(t);
    }), r;
  }function mt(t, n, r, e, u) {
    var i = -1,
        o = t.length;for (r || (r = Dn), u || (u = []); ++i < o;) {
      var a = t[i];n > 0 && r(a) ? n > 1 ? mt(a, n - 1, r, e, u) : c(u, a) : e || (u[u.length] = a);
    }return u;
  }function At(t, n) {
    return t && oo(t, n, Gr);
  }function Ot(t, n) {
    return i(n, function (n) {
      return Tr(t[n]);
    });
  }function kt(t, n) {
    n = Qt(n, t);for (var r = 0, e = n.length; null != t && r < e;) {
      t = t[or(n[r++])];
    }return r && r == e ? t : ce;
  }function xt(t, n, r) {
    var e = n(t);return jo(t) ? e : c(e, r(t));
  }function zt(t) {
    return null == t ? t === ce ? iu : Xe : Ti && Ti in Object(t) ? Ln(t) : tr(t);
  }function St(t, n) {
    return null != t && n in Object(t);
  }function $t(t, n, e) {
    n = Qt(n, t), t = rr(t, n);var u = null == t ? t : t[or(hr(n))];return null == u ? ce : r(u, t, e);
  }function Rt(t) {
    return Fr(t) && zt(t) == Me;
  }function Et(t, n, r, e, u) {
    return t === n || (null == t || null == n || !Fr(t) && !Fr(n) ? t !== t && n !== n : It(t, n, r, e, Et, u));
  }function It(t, n, r, e, u, i) {
    var o = jo(t),
        a = jo(n),
        c = o ? Ne : lo(t),
        f = a ? Ne : lo(n);c = c == Me ? Ye : c, f = f == Me ? Ye : f;var s = c == Ye,
        _ = f == Ye,
        l = c == f;if (l && mo(t)) {
      if (!mo(n)) return false;o = true, s = false;
    }if (l && !s) return i || (i = new ot()), o || Ao(t) ? An(t, n, r, e, u, i) : On(t, n, c, r, e, u, i);
    if (!(r & de)) {
      var h = s && di.call(t, "__wrapped__"),
          p = _ && di.call(n, "__wrapped__");if (h || p) {
        var v = h ? t.value() : t,
            y = p ? n.value() : n;return i || (i = new ot()), u(v, y, r, e, i);
      }
    }return !!l && (i || (i = new ot()), kn(t, n, r, e, u, i));
  }function Tt(t, n, r, e) {
    var u = r.length,
        i = u,
        o = !e;if (null == t) return !i;for (t = Object(t); u--;) {
      var a = r[u];if (o && a[2] ? a[1] !== t[a[0]] : !(a[0] in t)) return false;
    }for (; ++u < i;) {
      a = r[u];var c = a[0],
          f = t[c],
          s = a[1];if (o && a[2]) {
        if (f === ce && !(c in t)) return false;
      } else {
        var _ = new ot();if (e) var l = e(f, s, c, t, n, _);if (!(l === ce ? Et(s, f, de | be, e, _) : l)) return false;
      }
    }return true;
  }function Ct(t) {
    return !(!Lr(t) || Kn(t)) && (Tr(t) ? mi : Eu).test(ar(t));
  }function Lt(t) {
    return Fr(t) && Cr(t.length) && !!ni[zt(t)];
  }function Ft(t) {
    return typeof t == "function" ? t : null == t ? te : (typeof t === "undefined" ? "undefined" : _typeof(t)) == "object" ? jo(t) ? Mt(t[0], t[1]) : Wt(t) : ue(t);
  }function Bt(t) {
    if (!Hn(t)) return Bi(t);var n = [];for (var r in Object(t)) {
      di.call(t, r) && "constructor" != r && n.push(r);
    }return n;
  }function Pt(t) {
    if (!Lr(t)) return Zn(t);var n = Hn(t),
        r = [];for (var e in t) {
      ("constructor" != e || !n && di.call(t, e)) && r.push(e);
    }return r;
  }function Ut(t, n) {
    var r = -1,
        e = Ir(t) ? Array(t.length) : [];return io(t, function (t, u, i) {
      e[++r] = n(t, u, i);
    }), e;
  }function Wt(t) {
    var n = Tn(t);return 1 == n.length && n[0][2] ? Xn(n[0][0], n[0][1]) : function (r) {
      return r === t || Tt(r, t, n);
    };
  }function Mt(t, n) {
    return qn(t) && Qn(n) ? Xn(or(t), n) : function (r) {
      var e = Vr(r, t);return e === ce && e === n ? qr(r, t) : Et(n, e, de | be);
    };
  }function Nt(t, n, r) {
    var e = -1;return n = a(n.length ? n : [te], d(En())), y(Ut(t, function (t, r, u) {
      return { a: a(n, function (n) {
          return n(t);
        }), b: ++e, c: t };
    }), function (t, n) {
      return cn(t, n, r);
    });
  }function Dt(t, n, r) {
    for (var e = -1, u = n.length, i = {}; ++e < u;) {
      var o = n[e],
          a = kt(t, o);r(a, o) && Gt(i, Qt(o, t), a);
    }return i;
  }function Vt(t) {
    return function (n) {
      return kt(n, t);
    };
  }function qt(t, n) {
    return po(nr(t, n, te), t + "");
  }function Gt(t, n, r, e) {
    if (!Lr(t)) return t;n = Qt(n, t);for (var u = -1, i = n.length, o = i - 1, a = t; null != a && ++u < i;) {
      var c = or(n[u]),
          f = r;if (u != o) {
        var s = a[c];f = e ? e(s, c, a) : ce, f === ce && (f = Lr(s) ? s : Vn(n[u + 1]) ? [] : {});
      }ht(a, c, f), a = a[c];
    }return t;
  }function Jt(t, n, r) {
    var e = -1,
        u = t.length;n < 0 && (n = -n > u ? 0 : u + n), r = r > u ? u : r, r < 0 && (r += u), u = n > r ? 0 : r - n >>> 0, n >>>= 0;for (var i = Array(u); ++e < u;) {
      i[e] = t[e + n];
    }return i;
  }function Kt(t) {
    if (typeof t == "string") return t;if (jo(t)) return a(t, Kt) + "";if (Pr(t)) return eo ? eo.call(t) : "";var n = t + "";return "0" == n && 1 / t == -Le ? "-0" : n;
  }function Ht(t, n) {
    var r = t;return r instanceof L && (r = r.value()), f(n, function (t, n) {
      return n.func.apply(n.thisArg, c([t], n.args));
    }, r);
  }function Qt(t, n) {
    return jo(t) ? t : qn(t, n) ? [t] : vo(Dr(t));
  }function Xt(t, n, r) {
    var e = t.length;return r = r === ce ? e : r, !n && r >= e ? t : Jt(t, n, r);
  }function Yt(t, n) {
    if (n) return t.slice();var r = t.length,
        e = xi ? xi(r) : new t.constructor(r);
    return t.copy(e), e;
  }function Zt(t) {
    var n = new t.constructor(t.byteLength);return new ki(n).set(new ki(t)), n;
  }function tn(t, n) {
    return new t.constructor(n ? Zt(t.buffer) : t.buffer, t.byteOffset, t.byteLength);
  }function nn(n, r, e) {
    return f(r ? e(k(n), ve) : k(n), t, new n.constructor());
  }function rn(t) {
    var n = new t.constructor(t.source, Su.exec(t));return n.lastIndex = t.lastIndex, n;
  }function en(t, r, e) {
    return f(r ? e(S(t), ve) : S(t), n, new t.constructor());
  }function un(t) {
    return ro ? Object(ro.call(t)) : {};
  }function on(t, n) {
    return new t.constructor(n ? Zt(t.buffer) : t.buffer, t.byteOffset, t.length);
  }function an(t, n) {
    if (t !== n) {
      var r = t !== ce,
          e = null === t,
          u = t === t,
          i = Pr(t),
          o = n !== ce,
          a = null === n,
          c = n === n,
          f = Pr(n);if (!a && !f && !i && t > n || i && o && c && !a && !f || e && o && c || !r && c || !u) return 1;if (!e && !i && !f && t < n || f && r && u && !e && !i || a && r && u || !o && u || !c) return -1;
    }return 0;
  }function cn(t, n, r) {
    for (var e = -1, u = t.a, i = n.a, o = u.length, a = r.length; ++e < o;) {
      var c = an(u[e], i[e]);if (c) {
        if (e >= a) return c;return c * ("desc" == r[e] ? -1 : 1);
      }
    }return t.b - n.b;
  }function fn(t, n, r, e) {
    for (var u = -1, i = t.length, o = r.length, a = -1, c = n.length, f = Pi(i - o, 0), s = Array(c + f), _ = !e; ++a < c;) {
      s[a] = n[a];
    }for (; ++u < o;) {
      (_ || u < i) && (s[r[u]] = t[u]);
    }for (; f--;) {
      s[a++] = t[u++];
    }return s;
  }function sn(t, n, r, e) {
    for (var u = -1, i = t.length, o = -1, a = r.length, c = -1, f = n.length, s = Pi(i - a, 0), _ = Array(s + f), l = !e; ++u < s;) {
      _[u] = t[u];
    }for (var h = u; ++c < f;) {
      _[h + c] = n[c];
    }for (; ++o < a;) {
      (l || u < i) && (_[h + r[o]] = t[u++]);
    }return _;
  }function _n(t, n) {
    var r = -1,
        e = t.length;for (n || (n = Array(e)); ++r < e;) {
      n[r] = t[r];
    }return n;
  }function ln(t, n, r, e) {
    var u = !r;r || (r = {});for (var i = -1, o = n.length; ++i < o;) {
      var a = n[i],
          c = e ? e(r[a], t[a], a, r, t) : ce;c === ce && (c = t[a]), u ? dt(r, a, c) : ht(r, a, c);
    }return r;
  }function hn(t, n) {
    return ln(t, so(t), n);
  }function pn(t, n) {
    return ln(t, _o(t), n);
  }function vn(t, n) {
    return function (r, u) {
      var i = jo(r) ? e : vt,
          o = n ? n() : {};return i(r, t, En(u, 2), o);
    };
  }function yn(t, n) {
    return function (r, e) {
      if (null == r) return r;if (!Ir(r)) return t(r, e);for (var u = r.length, i = n ? u : -1, o = Object(r); (n ? i-- : ++i < u) && e(o[i], i, o) !== false;) {}return r;
    };
  }function gn(t) {
    return function (n, r, e) {
      for (var u = -1, i = Object(n), o = e(n), a = o.length; a--;) {
        var c = o[t ? a : ++u];if (r(i[c], c, i) === false) break;
      }return n;
    };
  }function dn(t) {
    return function (n) {
      n = Dr(n);var r = A(n) ? R(n) : ce,
          e = r ? r[0] : n.charAt(0),
          u = r ? Xt(r, 1).join("") : n.slice(1);return e[t]() + u;
    };
  }function bn(t) {
    return function () {
      var n = arguments;switch (n.length) {case 0:
          return new t();case 1:
          return new t(n[0]);case 2:
          return new t(n[0], n[1]);case 3:
          return new t(n[0], n[1], n[2]);case 4:
          return new t(n[0], n[1], n[2], n[3]);case 5:
          return new t(n[0], n[1], n[2], n[3], n[4]);case 6:
          return new t(n[0], n[1], n[2], n[3], n[4], n[5]);case 7:
          return new t(n[0], n[1], n[2], n[3], n[4], n[5], n[6]);}var r = uo(t.prototype),
          e = t.apply(r, n);
      return Lr(e) ? e : r;
    };
  }function wn(t) {
    return function (n, r, e) {
      var u = Object(n);if (!Ir(n)) {
        var i = En(r, 3);n = Gr(n), r = function r(t) {
          return i(u[t], t, u);
        };
      }var o = t(n, r, e);return o > -1 ? u[i ? n[o] : o] : ce;
    };
  }function jn(t, n, r, e, u, i, o, a, c, f) {
    function s() {
      for (var g = arguments.length, d = Array(g), b = g; b--;) {
        d[b] = arguments[b];
      }if (p) var w = Rn(s),
          m = j(d, w);if (e && (d = fn(d, e, u, p)), i && (d = sn(d, i, o, p)), g -= m, p && g < f) {
        return mn(t, n, jn, s.placeholder, r, d, z(d, w), a, c, f - g);
      }var A = l ? r : this,
          O = h ? A[t] : t;return g = d.length, a ? d = er(d, a) : v && g > 1 && d.reverse(), _ && c < g && (d.length = c), this && this !== oi && this instanceof s && (O = y || bn(O)), O.apply(A, d);
    }var _ = n & ze,
        l = n & we,
        h = n & je,
        p = n & (Ae | Oe),
        v = n & $e,
        y = h ? ce : bn(t);return s;
  }function mn(t, n, r, e, u, i, o, a, c, f) {
    var s = n & Ae,
        _ = s ? o : ce,
        l = s ? ce : o,
        h = s ? i : ce,
        p = s ? ce : i;n |= s ? ke : xe, n &= ~(s ? xe : ke), n & me || (n &= ~(we | je));var v = [t, n, u, h, _, p, l, a, c, f],
        y = r.apply(ce, v);return Jn(t) && ho(y, v), y.placeholder = e, ur(y, t, n);
  }function An(t, n, r, e, u, i) {
    var o = r & de,
        a = t.length,
        c = n.length;if (a != c && !(o && c > a)) return false;var f = i.get(t);if (f && i.get(n)) return f == n;var _ = -1,
        l = true,
        h = r & be ? new et() : ce;
    for (i.set(t, n), i.set(n, t); ++_ < a;) {
      var p = t[_],
          v = n[_];if (e) var y = o ? e(v, p, _, n, t, i) : e(p, v, _, t, n, i);if (y !== ce) {
        if (y) continue;l = false;break;
      }if (h) {
        if (!s(n, function (t, n) {
          if (!w(h, n) && (p === t || u(p, t, r, e, i))) return h.push(n);
        })) {
          l = false;break;
        }
      } else if (p !== v && !u(p, v, r, e, i)) {
        l = false;break;
      }
    }return i.delete(t), i.delete(n), l;
  }function On(t, n, r, e, u, i, o) {
    switch (r) {case cu:
        if (t.byteLength != n.byteLength || t.byteOffset != n.byteOffset) return false;t = t.buffer, n = n.buffer;case au:
        return !(t.byteLength != n.byteLength || !i(new ki(t), new ki(n)));
      case Ve:case qe:case Qe:
        return Er(+t, +n);case Ge:
        return t.name == n.name && t.message == n.message;case nu:case eu:
        return t == n + "";case He:
        var a = k;case ru:
        var c = e & de;if (a || (a = S), t.size != n.size && !c) return false;var f = o.get(t);if (f) return f == n;e |= be, o.set(t, n);var s = An(a(t), a(n), e, u, i, o);return o.delete(t), s;case uu:
        if (ro) return ro.call(t) == ro.call(n);}return false;
  }function kn(t, n, r, e, u, i) {
    var o = r & de,
        a = zn(t),
        c = a.length;if (c != zn(n).length && !o) return false;for (var f = c; f--;) {
      var s = a[f];if (!(o ? s in n : di.call(n, s))) return false;
    }var _ = i.get(t);if (_ && i.get(n)) return _ == n;var l = true;i.set(t, n), i.set(n, t);for (var h = o; ++f < c;) {
      s = a[f];var p = t[s],
          v = n[s];if (e) var y = o ? e(v, p, s, n, t, i) : e(p, v, s, t, n, i);if (!(y === ce ? p === v || u(p, v, r, e, i) : y)) {
        l = false;break;
      }h || (h = "constructor" == s);
    }if (l && !h) {
      var g = t.constructor,
          d = n.constructor;g != d && "constructor" in t && "constructor" in n && !(typeof g == "function" && g instanceof g && typeof d == "function" && d instanceof d) && (l = false);
    }return i.delete(t), i.delete(n), l;
  }function xn(t) {
    return po(nr(t, ce, lr), t + "");
  }function zn(t) {
    return xt(t, Gr, so);
  }function Sn(t) {
    return xt(t, Jr, _o);
  }function $n(t) {
    for (var n = t.name + "", r = Hi[n], e = di.call(Hi, n) ? r.length : 0; e--;) {
      var u = r[e],
          i = u.func;if (null == i || i == t) return u.name;
    }return n;
  }function Rn(t) {
    return (di.call(I, "placeholder") ? I : t).placeholder;
  }function En() {
    var t = I.iteratee || ne;return t = t === ne ? Ft : t, arguments.length ? t(arguments[0], arguments[1]) : t;
  }function In(t, n) {
    var r = t.__data__;return Gn(n) ? r[typeof n == "string" ? "string" : "hash"] : r.map;
  }function Tn(t) {
    for (var n = Gr(t), r = n.length; r--;) {
      var e = n[r],
          u = t[e];n[r] = [e, u, Qn(u)];
    }return n;
  }function Cn(t, n) {
    var r = m(t, n);return Ct(r) ? r : ce;
  }function Ln(t) {
    var n = di.call(t, Ti),
        r = t[Ti];try {
      t[Ti] = ce;var e = true;
    } catch (t) {}var u = ji.call(t);return e && (n ? t[Ti] = r : delete t[Ti]), u;
  }function Fn(t, n, r) {
    for (var e = -1, u = r.length; ++e < u;) {
      var i = r[e],
          o = i.size;switch (i.type) {case "drop":
          t += o;break;case "dropRight":
          n -= o;break;case "take":
          n = Ui(n, t + o);break;case "takeRight":
          t = Pi(t, n - o);}
    }return { start: t, end: n };
  }function Bn(t) {
    var n = t.match(ku);return n ? n[1].split(xu) : [];
  }function Pn(t, n, r) {
    n = Qt(n, t);for (var e = -1, u = n.length, i = false; ++e < u;) {
      var o = or(n[e]);if (!(i = null != t && r(t, o))) break;t = t[o];
    }return i || ++e != u ? i : (u = null == t ? 0 : t.length, !!u && Cr(u) && Vn(o, u) && (jo(t) || wo(t)));
  }function Un(t) {
    var n = t.length,
        r = t.constructor(n);return n && "string" == typeof t[0] && di.call(t, "index") && (r.index = t.index, r.input = t.input), r;
  }function Wn(t) {
    return typeof t.constructor != "function" || Hn(t) ? {} : uo(zi(t));
  }function Mn(t, n, r, e) {
    var u = t.constructor;switch (n) {case au:
        return Zt(t);case Ve:case qe:
        return new u(+t);case cu:
        return tn(t, e);case fu:case su:case _u:case lu:
      case hu:case pu:case vu:case yu:case gu:
        return on(t, e);case He:
        return nn(t, e, r);case Qe:case eu:
        return new u(t);case nu:
        return rn(t);case ru:
        return en(t, e, r);case uu:
        return un(t);}
  }function Nn(t, n) {
    var r = n.length;if (!r) return t;var e = r - 1;return n[e] = (r > 1 ? "& " : "") + n[e], n = n.join(r > 2 ? ", " : " "), t.replace(Ou, "{\n/* [wrapped with " + n + "] */\n");
  }function Dn(t) {
    return jo(t) || wo(t) || !!(Ei && t && t[Ei]);
  }function Vn(t, n) {
    return n = null == n ? Fe : n, !!n && (typeof t == "number" || Tu.test(t)) && t > -1 && t % 1 == 0 && t < n;
  }function qn(t, n) {
    if (jo(t)) return false;var r = typeof t === "undefined" ? "undefined" : _typeof(t);return !("number" != r && "symbol" != r && "boolean" != r && null != t && !Pr(t)) || bu.test(t) || !du.test(t) || null != n && t in Object(n);
  }function Gn(t) {
    var n = typeof t === "undefined" ? "undefined" : _typeof(t);return "string" == n || "number" == n || "symbol" == n || "boolean" == n ? "__proto__" !== t : null === t;
  }function Jn(t) {
    var n = $n(t),
        r = I[n];if (typeof r != "function" || !(n in L.prototype)) return false;if (t === r) return true;var e = fo(r);return !!e && t === e[0];
  }function Kn(t) {
    return !!wi && wi in t;
  }function Hn(t) {
    var n = t && t.constructor;return t === (typeof n == "function" && n.prototype || vi);
  }function Qn(t) {
    return t === t && !Lr(t);
  }function Xn(t, n) {
    return function (r) {
      return null != r && r[t] === n && (n !== ce || t in Object(r));
    };
  }function Yn(t) {
    var n = $r(t, function (t) {
      return r.size === he && r.clear(), t;
    }),
        r = n.cache;return n;
  }function Zn(t) {
    var n = [];if (null != t) for (var r in Object(t)) {
      n.push(r);
    }return n;
  }function tr(t) {
    return ji.call(t);
  }function nr(t, n, e) {
    return n = Pi(n === ce ? t.length - 1 : n, 0), function () {
      for (var u = arguments, i = -1, o = Pi(u.length - n, 0), a = Array(o); ++i < o;) {
        a[i] = u[n + i];
      }i = -1;for (var c = Array(n + 1); ++i < n;) {
        c[i] = u[i];
      }return c[n] = e(a), r(t, this, c);
    };
  }function rr(t, n) {
    return n.length < 2 ? t : kt(t, Jt(n, 0, -1));
  }function er(t, n) {
    for (var r = t.length, e = Ui(n.length, r), u = _n(t); e--;) {
      var i = n[e];t[e] = Vn(i, r) ? u[i] : ce;
    }return t;
  }function ur(t, n, r) {
    var e = n + "";return po(t, Nn(e, cr(Bn(e), r)));
  }function ir(t) {
    var n = 0,
        r = 0;return function () {
      var e = Wi(),
          u = Ee - (e - r);if (r = e, u > 0) {
        if (++n >= Re) return arguments[0];
      } else n = 0;return t.apply(ce, arguments);
    };
  }function or(t) {
    if (typeof t == "string" || Pr(t)) return t;var n = t + "";return "0" == n && 1 / t == -Le ? "-0" : n;
  }function ar(t) {
    if (null != t) {
      try {
        return gi.call(t);
      } catch (t) {}try {
        return t + "";
      } catch (t) {}
    }return "";
  }function cr(t, n) {
    return u(We, function (r) {
      var e = "_." + r[0];n & r[1] && !o(t, e) && t.push(e);
    }), t.sort();
  }function fr(t) {
    if (t instanceof L) return t.clone();var n = new C(t.__wrapped__, t.__chain__);return n.__actions__ = _n(t.__actions__), n.__index__ = t.__index__, n.__values__ = t.__values__, n;
  }function sr() {
    var t = arguments.length;if (!t) return [];for (var n = Array(t - 1), r = arguments[0], e = t; e--;) {
      n[e - 1] = arguments[e];
    }return c(jo(r) ? _n(r) : [r], mt(n, 1));
  }function _r(t, n, r) {
    var e = null == t ? 0 : t.length;if (!e) return -1;var u = null == r ? 0 : Mr(r);return u < 0 && (u = Pi(e + u, 0)), l(t, En(n, 3), u);
  }function lr(t) {
    return (null == t ? 0 : t.length) ? mt(t, 1) : [];
  }function hr(t) {
    var n = null == t ? 0 : t.length;return n ? t[n - 1] : ce;
  }function pr(t) {
    return null == t ? t : Mi.call(t);
  }function vr(t) {
    var n = I(t);return n.__chain__ = true, n;
  }function yr(t, n) {
    return n(t), t;
  }function gr(t, n) {
    return n(t);
  }function dr() {
    return vr(this);
  }function br() {
    return new C(this.value(), this.__chain__);
  }function wr() {
    this.__values__ === ce && (this.__values__ = Ur(this.value()));
    var t = this.__index__ >= this.__values__.length;return { done: t, value: t ? ce : this.__values__[this.__index__++] };
  }function jr() {
    return this;
  }function mr(t) {
    for (var n, r = this; r instanceof T;) {
      var e = fr(r);e.__index__ = 0, e.__values__ = ce, n ? u.__wrapped__ = e : n = e;var u = e;r = r.__wrapped__;
    }return u.__wrapped__ = t, n;
  }function Ar() {
    var t = this.__wrapped__;if (t instanceof L) {
      var n = t;return this.__actions__.length && (n = new L(this)), n = n.reverse(), n.__actions__.push({ func: gr, args: [pr], thisArg: ce }), new C(n, this.__chain__);
    }return this.thru(pr);
  }function Or() {
    return Ht(this.__wrapped__, this.__actions__);
  }function kr(t, n) {
    return (jo(t) ? i : jt)(t, En(n, 3));
  }function xr(t, n) {
    return (jo(t) ? a : Ut)(t, En(n, 3));
  }function zr(t, n, r, e) {
    return null == t ? [] : (jo(n) || (n = null == n ? [] : [n]), r = e ? ce : r, jo(r) || (r = null == r ? [] : [r]), Nt(t, n, r));
  }function Sr(t, n, r) {
    function e(n) {
      var r = l,
          e = h;return l = h = ce, d = n, v = t.apply(e, r);
    }function u(t) {
      return d = t, y = setTimeout(a, n), b ? e(t) : v;
    }function i(t) {
      var r = t - g,
          e = t - d,
          u = n - r;return w ? Ui(u, p - e) : u;
    }function o(t) {
      var r = t - g,
          e = t - d;return g === ce || r >= n || r < 0 || w && e >= p;
    }function a() {
      var t = bo();return o(t) ? c(t) : (y = setTimeout(a, i(t)), ce);
    }function c(t) {
      return y = ce, j && l ? e(t) : (l = h = ce, v);
    }function f() {
      y !== ce && clearTimeout(y), d = 0, l = g = h = y = ce;
    }function s() {
      return y === ce ? v : c(bo());
    }function _() {
      var t = bo(),
          r = o(t);if (l = arguments, h = this, g = t, r) {
        if (y === ce) return u(g);if (w) return y = setTimeout(a, n), e(g);
      }return y === ce && (y = setTimeout(a, n)), v;
    }var l,
        h,
        p,
        v,
        y,
        g,
        d = 0,
        b = false,
        w = false,
        j = true;if (typeof t != "function") throw new TypeError(_e);return n = Nr(n) || 0, Lr(r) && (b = !!r.leading, w = "maxWait" in r, p = w ? Pi(Nr(r.maxWait) || 0, n) : p, j = "trailing" in r ? !!r.trailing : j), _.cancel = f, _.flush = s, _;
  }function $r(t, n) {
    if (typeof t != "function" || null != n && typeof n != "function") throw new TypeError(_e);var r = function r() {
      var e = arguments,
          u = n ? n.apply(this, e) : e[0],
          i = r.cache;if (i.has(u)) return i.get(u);var o = t.apply(this, e);return r.cache = i.set(u, o) || i, o;
    };return r.cache = new ($r.Cache || X)(), r;
  }function Rr(t) {
    if (typeof t != "function") throw new TypeError(_e);return function () {
      var n = arguments;switch (n.length) {case 0:
          return !t.call(this);case 1:
          return !t.call(this, n[0]);
        case 2:
          return !t.call(this, n[0], n[1]);case 3:
          return !t.call(this, n[0], n[1], n[2]);}return !t.apply(this, n);
    };
  }function Er(t, n) {
    return t === n || t !== t && n !== n;
  }function Ir(t) {
    return null != t && Cr(t.length) && !Tr(t);
  }function Tr(t) {
    if (!Lr(t)) return false;var n = zt(t);return n == Je || n == Ke || n == De || n == tu;
  }function Cr(t) {
    return typeof t == "number" && t > -1 && t % 1 == 0 && t <= Fe;
  }function Lr(t) {
    var n = typeof t === "undefined" ? "undefined" : _typeof(t);return null != t && ("object" == n || "function" == n);
  }function Fr(t) {
    return null != t && (typeof t === "undefined" ? "undefined" : _typeof(t)) == "object";
  }function Br(t) {
    return typeof t == "string" || !jo(t) && Fr(t) && zt(t) == eu;
  }function Pr(t) {
    return (typeof t === "undefined" ? "undefined" : _typeof(t)) == "symbol" || Fr(t) && zt(t) == uu;
  }function Ur(t) {
    if (!t) return [];if (Ir(t)) return Br(t) ? R(t) : _n(t);if (Ii && t[Ii]) return O(t[Ii]());var n = lo(t);return (n == He ? k : n == ru ? S : Qr)(t);
  }function Wr(t) {
    if (!t) return 0 === t ? t : 0;if (t = Nr(t), t === Le || t === -Le) {
      return (t < 0 ? -1 : 1) * Be;
    }return t === t ? t : 0;
  }function Mr(t) {
    var n = Wr(t),
        r = n % 1;return n === n ? r ? n - r : n : 0;
  }function Nr(t) {
    if (typeof t == "number") return t;if (Pr(t)) return Pe;if (Lr(t)) {
      var n = typeof t.valueOf == "function" ? t.valueOf() : t;t = Lr(n) ? n + "" : n;
    }if (typeof t != "string") return 0 === t ? t : +t;
    t = t.replace(Au, "");var r = Ru.test(t);return r || Iu.test(t) ? ei(t.slice(2), r ? 2 : 8) : $u.test(t) ? Pe : +t;
  }function Dr(t) {
    return null == t ? "" : Kt(t);
  }function Vr(t, n, r) {
    var e = null == t ? ce : kt(t, n);return e === ce ? r : e;
  }function qr(t, n) {
    return null != t && Pn(t, n, St);
  }function Gr(t) {
    return Ir(t) ? lt(t) : Bt(t);
  }function Jr(t) {
    return Ir(t) ? lt(t, true) : Pt(t);
  }function Kr(t, n) {
    var r = {};return n = En(n, 3), At(t, function (t, e, u) {
      dt(r, e, n(t, e, u));
    }), r;
  }function Hr(t, n) {
    if (null == t) return {};var r = a(Sn(t), function (t) {
      return [t];
    });return n = En(n), Dt(t, r, function (t, r) {
      return n(t, r[0]);
    });
  }function Qr(t) {
    return null == t ? [] : b(t, Gr(t));
  }function Xr(t) {
    return Oo(Dr(t).toLowerCase());
  }function Yr() {
    var t = arguments,
        n = Dr(t[0]);return t.length < 3 ? n : n.replace(t[1], t[2]);
  }function Zr(t) {
    return function () {
      return t;
    };
  }function te(t) {
    return t;
  }function ne(t) {
    return Ft(typeof t == "function" ? t : wt(t, ve));
  }function re(t, n, r) {
    var e = Gr(n),
        i = Ot(n, e);null != r || Lr(n) && (i.length || !e.length) || (r = n, n = t, t = this, i = Ot(n, Gr(n)));var o = !(Lr(r) && "chain" in r && !r.chain),
        a = Tr(t);return u(i, function (r) {
      var e = n[r];
      t[r] = e, a && (t.prototype[r] = function () {
        var n = this.__chain__;if (o || n) {
          var r = t(this.__wrapped__);return (r.__actions__ = _n(this.__actions__)).push({ func: e, args: arguments, thisArg: t }), r.__chain__ = n, r;
        }return e.apply(t, c([this.value()], arguments));
      });
    }), t;
  }function ee() {}function ue(t) {
    return qn(t) ? v(or(t)) : Vt(t);
  }function ie() {
    return [];
  }function oe() {
    return false;
  }function ae(t) {
    var n = ++bi;return Dr(t) + n;
  }var ce,
      fe = "4.17.4",
      se = 200,
      _e = "Expected a function",
      le = "__lodash_hash_undefined__",
      he = 500,
      pe = "__lodash_placeholder__",
      ve = 1,
      ye = 2,
      ge = 4,
      de = 1,
      be = 2,
      we = 1,
      je = 2,
      me = 4,
      Ae = 8,
      Oe = 16,
      ke = 32,
      xe = 64,
      ze = 128,
      Se = 256,
      $e = 512,
      Re = 800,
      Ee = 16,
      Ie = 1,
      Te = 2,
      Ce = 3,
      Le = 1 / 0,
      Fe = 9007199254740991,
      Be = 1.7976931348623157e308,
      Pe = NaN,
      Ue = 4294967295,
      We = [["ary", ze], ["bind", we], ["bindKey", je], ["curry", Ae], ["curryRight", Oe], ["flip", $e], ["partial", ke], ["partialRight", xe], ["rearg", Se]],
      Me = "[object Arguments]",
      Ne = "[object Array]",
      De = "[object AsyncFunction]",
      Ve = "[object Boolean]",
      qe = "[object Date]",
      Ge = "[object Error]",
      Je = "[object Function]",
      Ke = "[object GeneratorFunction]",
      He = "[object Map]",
      Qe = "[object Number]",
      Xe = "[object Null]",
      Ye = "[object Object]",
      Ze = "[object Promise]",
      tu = "[object Proxy]",
      nu = "[object RegExp]",
      ru = "[object Set]",
      eu = "[object String]",
      uu = "[object Symbol]",
      iu = "[object Undefined]",
      ou = "[object WeakMap]",
      au = "[object ArrayBuffer]",
      cu = "[object DataView]",
      fu = "[object Float32Array]",
      su = "[object Float64Array]",
      _u = "[object Int8Array]",
      lu = "[object Int16Array]",
      hu = "[object Int32Array]",
      pu = "[object Uint8Array]",
      vu = "[object Uint8ClampedArray]",
      yu = "[object Uint16Array]",
      gu = "[object Uint32Array]",
      du = /\.|\[(?:[^[\]]*|(["'])(?:(?!\1)[^\\]|\\.)*?\1)\]/,
      bu = /^\w*$/,
      wu = /^\./,
      ju = /[^.[\]]+|\[(?:(-?\d+(?:\.\d+)?)|(["'])((?:(?!\2)[^\\]|\\.)*?)\2)\]|(?=(?:\.|\[\])(?:\.|\[\]|$))/g,
      mu = /[\\^$.*+?()[\]{}|]/g,
      Au = /^\s+|\s+$/g,
      Ou = /\{(?:\n\/\* \[wrapped with .+\] \*\/)?\n?/,
      ku = /\{\n\/\* \[wrapped with (.+)\] \*/,
      xu = /,? & /,
      zu = /\\(\\)?/g,
      Su = /\w*$/,
      $u = /^[-+]0x[0-9a-f]+$/i,
      Ru = /^0b[01]+$/i,
      Eu = /^\[object .+?Constructor\]$/,
      Iu = /^0o[0-7]+$/i,
      Tu = /^(?:0|[1-9]\d*)$/,
      Cu = "\\ud800-\\udfff",
      Lu = "\\u0300-\\u036f",
      Fu = "\\ufe20-\\ufe2f",
      Bu = "\\u20d0-\\u20ff",
      Pu = Lu + Fu + Bu,
      Uu = "\\ufe0e\\ufe0f",
      Wu = "[" + Cu + "]",
      Mu = "[" + Pu + "]",
      Nu = "\\ud83c[\\udffb-\\udfff]",
      Du = "(?:" + Mu + "|" + Nu + ")",
      Vu = "[^" + Cu + "]",
      qu = "(?:\\ud83c[\\udde6-\\uddff]){2}",
      Gu = "[\\ud800-\\udbff][\\udc00-\\udfff]",
      Ju = "\\u200d",
      Ku = Du + "?",
      Hu = "[" + Uu + "]?",
      Qu = "(?:" + Ju + "(?:" + [Vu, qu, Gu].join("|") + ")" + Hu + Ku + ")*",
      Xu = Hu + Ku + Qu,
      Yu = "(?:" + [Vu + Mu + "?", Mu, qu, Gu, Wu].join("|") + ")",
      Zu = RegExp(Nu + "(?=" + Nu + ")|" + Yu + Xu, "g"),
      ti = RegExp("[" + Ju + Cu + Pu + Uu + "]"),
      ni = {};
  ni[fu] = ni[su] = ni[_u] = ni[lu] = ni[hu] = ni[pu] = ni[vu] = ni[yu] = ni[gu] = true, ni[Me] = ni[Ne] = ni[au] = ni[Ve] = ni[cu] = ni[qe] = ni[Ge] = ni[Je] = ni[He] = ni[Qe] = ni[Ye] = ni[nu] = ni[ru] = ni[eu] = ni[ou] = false;var ri = {};ri[Me] = ri[Ne] = ri[au] = ri[cu] = ri[Ve] = ri[qe] = ri[fu] = ri[su] = ri[_u] = ri[lu] = ri[hu] = ri[He] = ri[Qe] = ri[Ye] = ri[nu] = ri[ru] = ri[eu] = ri[uu] = ri[pu] = ri[vu] = ri[yu] = ri[gu] = true, ri[Ge] = ri[Je] = ri[ou] = false;var ei = parseInt,
      ui = (typeof global === "undefined" ? "undefined" : _typeof(global)) == "object" && global && global.Object === Object && global,
      ii = (typeof self === "undefined" ? "undefined" : _typeof(self)) == "object" && self && self.Object === Object && self,
      oi = ui || ii || Function("return this")(),
      ai = (typeof exports === "undefined" ? "undefined" : _typeof(exports)) == "object" && exports && !exports.nodeType && exports,
      ci = ai && (typeof module === "undefined" ? "undefined" : _typeof(module)) == "object" && module && !module.nodeType && module,
      fi = ci && ci.exports === ai,
      si = fi && ui.process,
      _i = function () {
    try {
      return si && si.binding && si.binding("util");
    } catch (t) {}
  }(),
      li = _i && _i.isTypedArray,
      hi = Array.prototype,
      pi = Function.prototype,
      vi = Object.prototype,
      yi = oi["__core-js_shared__"],
      gi = pi.toString,
      di = vi.hasOwnProperty,
      bi = 0,
      wi = function () {
    var t = /[^.]+$/.exec(yi && yi.keys && yi.keys.IE_PROTO || "");return t ? "Symbol(src)_1." + t : "";
  }(),
      ji = vi.toString,
      mi = RegExp("^" + gi.call(di).replace(mu, "\\$&").replace(/hasOwnProperty|(function).*?(?=\\\()| for .+?(?=\\\])/g, "$1.*?") + "$"),
      Ai = fi ? oi.Buffer : ce,
      Oi = oi.Symbol,
      ki = oi.Uint8Array,
      xi = Ai ? Ai.allocUnsafe : ce,
      zi = x(Object.getPrototypeOf, Object),
      Si = Object.create,
      $i = vi.propertyIsEnumerable,
      Ri = hi.splice,
      Ei = Oi ? Oi.isConcatSpreadable : ce,
      Ii = Oi ? Oi.iterator : ce,
      Ti = Oi ? Oi.toStringTag : ce,
      Ci = function () {
    try {
      var t = Cn(Object, "defineProperty");return t({}, "", {}), t;
    } catch (t) {}
  }(),
      Li = Object.getOwnPropertySymbols,
      Fi = Ai ? Ai.isBuffer : ce,
      Bi = x(Object.keys, Object),
      Pi = Math.max,
      Ui = Math.min,
      Wi = Date.now,
      Mi = hi.reverse,
      Ni = Cn(oi, "DataView"),
      Di = Cn(oi, "Map"),
      Vi = Cn(oi, "Promise"),
      qi = Cn(oi, "Set"),
      Gi = Cn(oi, "WeakMap"),
      Ji = Cn(Object, "create"),
      Ki = Gi && new Gi(),
      Hi = {},
      Qi = ar(Ni),
      Xi = ar(Di),
      Yi = ar(Vi),
      Zi = ar(qi),
      to = ar(Gi),
      no = Oi ? Oi.prototype : ce,
      ro = no ? no.valueOf : ce,
      eo = no ? no.toString : ce,
      uo = function () {
    function t() {}return function (n) {
      if (!Lr(n)) return {};
      if (Si) return Si(n);t.prototype = n;var r = new t();return t.prototype = ce, r;
    };
  }();I.prototype = T.prototype, I.prototype.constructor = I, C.prototype = uo(T.prototype), C.prototype.constructor = C, L.prototype = uo(T.prototype), L.prototype.constructor = L, U.prototype.clear = W, U.prototype.delete = M, U.prototype.get = N, U.prototype.has = D, U.prototype.set = V, q.prototype.clear = G, q.prototype.delete = J, q.prototype.get = K, q.prototype.has = H, q.prototype.set = Q, X.prototype.clear = Y, X.prototype.delete = Z, X.prototype.get = tt, X.prototype.has = nt, X.prototype.set = rt, et.prototype.add = et.prototype.push = ut, et.prototype.has = it, ot.prototype.clear = at, ot.prototype.delete = ct, ot.prototype.get = ft, ot.prototype.has = st, ot.prototype.set = _t;var io = yn(At),
      oo = gn(),
      ao = Ki ? function (t, n) {
    return Ki.set(t, n), t;
  } : te,
      co = Ci ? function (t, n) {
    return Ci(t, "toString", { configurable: true, enumerable: false, value: Zr(n), writable: true });
  } : te,
      fo = Ki ? function (t) {
    return Ki.get(t);
  } : ee,
      so = Li ? function (t) {
    return null == t ? [] : (t = Object(t), i(Li(t), function (n) {
      return $i.call(t, n);
    }));
  } : ie,
      _o = Li ? function (t) {
    for (var n = []; t;) {
      c(n, so(t)), t = zi(t);
    }return n;
  } : ie,
      lo = zt;(Ni && lo(new Ni(new ArrayBuffer(1))) != cu || Di && lo(new Di()) != He || Vi && lo(Vi.resolve()) != Ze || qi && lo(new qi()) != ru || Gi && lo(new Gi()) != ou) && (lo = function lo(t) {
    var n = zt(t),
        r = n == Ye ? t.constructor : ce,
        e = r ? ar(r) : "";if (e) switch (e) {case Qi:
        return cu;case Xi:
        return He;case Yi:
        return Ze;case Zi:
        return ru;case to:
        return ou;}return n;
  });var ho = ir(ao),
      po = ir(co),
      vo = Yn(function (t) {
    var n = [];return wu.test(t) && n.push(""), t.replace(ju, function (t, r, e, u) {
      n.push(e ? u.replace(zu, "$1") : r || t);
    }), n;
  }),
      yo = (xn(function (t) {
    var n = t.length,
        r = n ? t[0] : 0,
        e = this.__wrapped__,
        u = function u(n) {
      return bt(n, t);
    };return !(n > 1 || this.__actions__.length) && e instanceof L && Vn(r) ? (e = e.slice(r, +r + (n ? 1 : 0)), e.__actions__.push({ func: gr, args: [u], thisArg: ce }), new C(e, this.__chain__).thru(function (t) {
      return n && !t.length && t.push(ce), t;
    })) : this.thru(u);
  }), wn(_r)),
      go = vn(function (t, n, r) {
    di.call(t, r) ? t[r].push(n) : dt(t, r, [n]);
  }),
      bo = function bo() {
    return oi.Date.now();
  };$r.Cache = X;var wo = Rt(function () {
    return arguments;
  }()) ? Rt : function (t) {
    return Fr(t) && di.call(t, "callee") && !$i.call(t, "callee");
  },
      jo = Array.isArray,
      mo = Fi || oe,
      Ao = li ? d(li) : Lt,
      Oo = dn("toUpperCase");I.chain = vr, I.concat = sr, I.constant = Zr, I.debounce = Sr, I.filter = kr, I.flatten = lr, I.groupBy = go, I.iteratee = ne, I.keys = Gr, I.keysIn = Jr, I.map = xr, I.mapValues = Kr, I.memoize = $r, I.mixin = re, I.negate = Rr, I.orderBy = zr, I.pickBy = Hr, I.property = ue, I.reverse = pr, I.tap = yr, I.thru = gr, I.toArray = Ur, I.values = Qr, re(I, I), I.capitalize = Xr, I.eq = Er, I.find = yo, I.findIndex = _r, I.get = Vr, I.hasIn = qr, I.identity = te, I.isArguments = wo, I.isArray = jo, I.isArrayLike = Ir, I.isBuffer = mo, I.isFunction = Tr, I.isLength = Cr, I.isObject = Lr, I.isObjectLike = Fr, I.isString = Br, I.isSymbol = Pr, I.isTypedArray = Ao, I.last = hr, I.stubArray = ie, I.stubFalse = oe, I.noop = ee, I.now = bo, I.replace = Yr, I.toFinite = Wr, I.toInteger = Mr, I.toNumber = Nr, I.toString = Dr, I.uniqueId = ae, I.upperFirst = Oo, re(I, function () {
    var t = {};return At(I, function (n, r) {
      di.call(I.prototype, r) || (t[r] = n);
    }), t;
  }(), { chain: false }), I.VERSION = fe, u(["drop", "take"], function (t, n) {
    L.prototype[t] = function (r) {
      r = r === ce ? 1 : Pi(Mr(r), 0);var e = this.__filtered__ && !n ? new L(this) : this.clone();
      return e.__filtered__ ? e.__takeCount__ = Ui(r, e.__takeCount__) : e.__views__.push({ size: Ui(r, Ue), type: t + (e.__dir__ < 0 ? "Right" : "") }), e;
    }, L.prototype[t + "Right"] = function (n) {
      return this.reverse()[t](n).reverse();
    };
  }), u(["filter", "map", "takeWhile"], function (t, n) {
    var r = n + 1,
        e = r == Ie || r == Ce;L.prototype[t] = function (t) {
      var n = this.clone();return n.__iteratees__.push({ iteratee: En(t, 3), type: r }), n.__filtered__ = n.__filtered__ || e, n;
    };
  }), u(["head", "last"], function (t, n) {
    var r = "take" + (n ? "Right" : "");L.prototype[t] = function () {
      return this[r](1).value()[0];
    };
  }), u(["initial", "tail"], function (t, n) {
    var r = "drop" + (n ? "" : "Right");L.prototype[t] = function () {
      return this.__filtered__ ? new L(this) : this[r](1);
    };
  }), L.prototype.compact = function () {
    return this.filter(te);
  }, L.prototype.find = function (t) {
    return this.filter(t).head();
  }, L.prototype.findLast = function (t) {
    return this.reverse().find(t);
  }, L.prototype.invokeMap = qt(function (t, n) {
    return typeof t == "function" ? new L(this) : this.map(function (r) {
      return $t(r, t, n);
    });
  }), L.prototype.reject = function (t) {
    return this.filter(Rr(En(t)));
  }, L.prototype.slice = function (t, n) {
    t = Mr(t);var r = this;return r.__filtered__ && (t > 0 || n < 0) ? new L(r) : (t < 0 ? r = r.takeRight(-t) : t && (r = r.drop(t)), n !== ce && (n = Mr(n), r = n < 0 ? r.dropRight(-n) : r.take(n - t)), r);
  }, L.prototype.takeRightWhile = function (t) {
    return this.reverse().takeWhile(t).reverse();
  }, L.prototype.toArray = function () {
    return this.take(Ue);
  }, At(L.prototype, function (t, n) {
    var r = /^(?:filter|find|map|reject)|While$/.test(n),
        e = /^(?:head|last)$/.test(n),
        u = I[e ? "take" + ("last" == n ? "Right" : "") : n],
        i = e || /^find/.test(n);u && (I.prototype[n] = function () {
      var n = this.__wrapped__,
          o = e ? [1] : arguments,
          a = n instanceof L,
          f = o[0],
          s = a || jo(n),
          _ = function _(t) {
        var n = u.apply(I, c([t], o));return e && l ? n[0] : n;
      };s && r && typeof f == "function" && 1 != f.length && (a = s = false);var l = this.__chain__,
          h = !!this.__actions__.length,
          p = i && !l,
          v = a && !h;if (!i && s) {
        n = v ? n : new L(this);var y = t.apply(n, o);return y.__actions__.push({ func: gr, args: [_], thisArg: ce }), new C(y, l);
      }return p && v ? t.apply(this, o) : (y = this.thru(_), p ? e ? y.value()[0] : y.value() : y);
    });
  }), u(["pop", "push", "shift", "sort", "splice", "unshift"], function (t) {
    var n = hi[t],
        r = /^(?:push|sort|unshift)$/.test(t) ? "tap" : "thru",
        e = /^(?:pop|shift)$/.test(t);I.prototype[t] = function () {
      var t = arguments;if (e && !this.__chain__) {
        var u = this.value();return n.apply(jo(u) ? u : [], t);
      }return this[r](function (r) {
        return n.apply(jo(r) ? r : [], t);
      });
    };
  }), At(L.prototype, function (t, n) {
    var r = I[n];if (r) {
      var e = r.name + "";(Hi[e] || (Hi[e] = [])).push({ name: n, func: r });
    }
  }), Hi[jn(ce, je).name] = [{ name: "wrapper", func: ce }], L.prototype.clone = F, L.prototype.reverse = B, L.prototype.value = P, I.prototype.chain = dr, I.prototype.commit = br, I.prototype.next = wr, I.prototype.plant = mr, I.prototype.reverse = Ar, I.prototype.toJSON = I.prototype.valueOf = I.prototype.value = Or, I.prototype.first = I.prototype.head, Ii && (I.prototype[Ii] = jr), typeof define == "function" && _typeof(define.amd) == "object" && define.amd ? (oi._ = I, define(function () {
    return I;
  })) : ci ? ((ci.exports = I)._ = I, ai._ = I) : oi._ = I;
}).call(undefined);
'use strict';

/**
 * @name          : PageTour.js Themes - Collections
 * @version       : 1.1.1
 * @cat           : Plugin/Widget/Tour/Onboard/Engage/Guide
 * @author        : Gamaki Studio (Gael Mamadi Stucki)
 * @documentation : http://jquery-widget.pagetourjs.com
 * @website       : http://www.pagetourjs.com
 * @copyright     : (c) 2017 Gamaki Studio (Gael Mamadi Stucki)
 */

;(function ($) {
  'use strict';

  /******************************/
  /*** WINDOW OBJECT EXPOSURE ***/
  /******************************/

  window.$.ptJsThemes = {
    'classic': {
      templateViewContinue: '\n        <div class="step-container" ptjs-id="container">\n          <span class="step-button-close" ptjs-id="button-close" ptjs-data="button-close"></span>\n          <div class="step-header" ptjs-id="header">\n            <div class="title" ptjs-data="title-continue"></div>\n          </div>\n          <div class="step-body" ptjs-id="body">\n            <div class="content" ptjs-data="content-continue"></div>\n          </div>\n          <div class="step-footer" ptjs-id="footer">\n            <div class="step-buttons">\n              <span ptjs-id="button-restart" ptjs-data="button-restart"></span>\n              <span ptjs-id="button-continue" ptjs-data="button-continue"></span>\n            </div>\n          </div>\n        </div>',
      templateViewMarker: '<span class="step-marker-action"></span>',
      templateViewWindow: '\n        <div class="step-container" ptjs-id="container">\n          <span class="step-button-close" ptjs-id="button-close" ptjs-data="button-close"></span>\n          <div class="step-header" ptjs-id="header">\n            <div class="title" ptjs-data="title"></div>\n          </div>\n          <div class="step-body" ptjs-id="body">\n            <div ptjs-id="content" class="content" ptjs-data="content"></div>\n          </div>\n          <div class="step-footer" ptjs-id="footer">\n            <span class="step-pagination" ptjs-id="pagination" ptjs-data="pagination"></span>\n            <div class="step-buttons">\n              <span ptjs-id="button-start" ptjs-data="button-start"></span>\n              <span ptjs-id="button-previous" ptjs-data="button-previous"></span>\n              <span ptjs-id="button-next" ptjs-data="button-next"></span>\n              <span ptjs-id="button-end" ptjs-data="button-end"></span>\n            </div>\n          </div>\n        </div>'
    }
  };
})(jQuery);
'use strict';

/**
 * @name          : PageTour.js Core - jQuery Plugin
 * @version       : 1.1.1
 * @cat           : Plugin/Widget/Tour/Onboard/Engage/Guide
 * @author        : Gamaki Studio (Gael Mamadi Stucki)
 * @documentation : http://jquery-widget.pagetourjs.com
 * @website       : http://www.pagetourjs.com
 * @copyright     : (c) 2017 Gamaki Studio (Gael Mamadi Stucki)
 */

;(function ($, window, document, _) {
  'use strict';

  /** Constant to define the ID to use when creating WRAPPER. */

  var _WRAPPER_ID = '_PTJS_WRAPPER';

  /***********************************/
  /*** ptJsCore PLUGIN CONSTRUCTOR ***/
  /***********************************/
  function Plugin() {}

  /********************************/
  /*** ptJsCore CLASS EXTENSION ***/
  /********************************/
  $.extend(Plugin.prototype, {
    version: "1.1.0",
    info: {},
    $body: null,
    $document: $(document),
    $window: $(window),
    $wrapper: {},

    /**
     * Animate the DOM element
     *
     * @public
     * @param {jQuery Element} $el The jQuery element.
     * @param {String} name The animation name.
     * @param {Function} cb Call-back function when animation ends.
     */
    animate: function animate($el, name, cb) {
      var animationEnd = 'webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend';
      $el.addClass('animated ' + name).one(animationEnd, function () {
        $el.removeClass('animated ' + name);
        if (cb) cb();
      });
    },

    /**
     * Create DOM wrapper
     *
     * @public
     * @param {String} id The ID of the DOM element.
     * @param {jQuery Element} parent The parent jQuery element.
     * @returns {jQuery Element} Returns the new DOM wrapper element.
     */
    createDomWrapper: function createDomWrapper(id, parent) {
      if ($('#' + id).length === 0) return $('<div>', { id: id }).prependTo(parent);else return $('#' + id);
    },

    /**
     * Check if DOM element exist
     *
     * @public
     * @param {String} el The DOM element selector.
     * @returns {Boolean} Returns true of false.
     */
    domExist: function domExist(el) {
      return $(el).length !== 0;
    },

    /**
     * Get details (size, height, position, etc...) of the DOM element.
     *
     * @public
     * @param {jQuery Element} $el The jQuery element.
     * @param {Number} padding Padding value to add for position calculation.
     * @returns {Object} Returns object.
     */
    getDetails: function getDetails($el, padding) {
      var elSize = {
        fullHeight: this.isDocOrWin($el) ? $el.innerHeight() : $el.outerHeight(),
        fullWidth: this.isDocOrWin($el) ? $el.innerWidth() : $el.outerWidth(),
        height: this.roundTo($el.height()),
        width: $el.width()
      },
          elOverflow = {
        height: $el[0].offsetHeight < $el[0].scrollHeight,
        width: $el[0].offsetWidth < $el[0].scrollWidth
      },
          elOffset = this.isDocOrWin($el) ? { top: 0, left: 0 } : $el.offset(),
          padding = padding || 0;

      return {
        overflow: elOverflow,
        position: {
          bottom: elOffset.top + elSize.fullHeight + padding,
          center: elOffset.left + elSize.fullWidth / 2,
          left: elOffset.left - padding,
          middle: elOffset.top + elSize.fullHeight / 2,
          right: elOffset.left + elSize.fullWidth + padding,
          scrollBottom: $el.scrollTop() + elSize.fullHeight,
          scrollTop: $el.scrollTop(),
          top: elOffset.top - padding
        },
        size: elSize
      };
    },

    /**
     * Get exceed positions of the DOM element relative to the window.
     *
     * @public
     * @param {jQuery Element} $el The jQuery element.
     * @param {Number} padding Padding value to add for exceed positions calculation.
     * @returns {Object} Returns object.
     */
    getExceedPosition: function getExceedPosition($el, padding) {
      var winDetails = this.getDetails(this.$window),
          elDetails = this.getDetails($el),
          exceedBottom = elDetails.position.bottom - winDetails.position.scrollTop >= winDetails.size.height - padding,
          exceedLeft = elDetails.position.left <= padding,
          exceedRight = elDetails.position.right >= winDetails.size.width - padding,
          exceedTop = elDetails.position.top <= padding + winDetails.position.scrollTop;

      return { bottom: exceedBottom, left: exceedLeft, right: exceedRight, top: exceedTop };
    },

    /**
     * Get value of stored key in localStorage.
     *
     * @public
     * @param {String} key The key to get.
     * @returns {Object} Returns value of the key.
     */
    getStorageItem: function getStorageItem(key) {
      if (this.hasStorage()) return localStorage.getItem(key);
    },

    /**
     * Check if jQuery element has exceed window dimension.
     *
     * @public
     * @param {jQuery Element} $el The jQuery element.
     * @param {Number} padding Padding value to add for exceed calculation.
     * @param {String} only Sets the axe to check : both || x (left,right) || y (top,bottom).
     * @returns {Boolean} Returns true of false.
     */
    hasExceed: function hasExceed($el, padding, only) {
      var exceed = this.getExceedPosition($el, padding);
      if (only === 'x') return exceed.left || exceed.right;else if (only === 'y') return exceed.bottom || exceed.top;
      return exceed.bottom || exceed.left || exceed.right || exceed.top;
    },

    /**
     * Check if browser has localStorage supports.
     *
     * @public
     * @returns {Boolean} Returns true of false.
     */
    hasStorage: function hasStorage() {
      return typeof Storage !== "undefined";
    },

    /**
     * Merge two objects in one.
     *
     * @public
     * @param {Object} opt1 Object one
     * @param {Object} opt2 Object two
     * @returns {Object} Returns the new merged object.
     */
    mergeOptions: function mergeOptions(opt1, opt2) {
      return $.extend(true, {}, opt1 || {}, opt2 || {});
    },

    /**
     * Check if DOM element is Document or Window.
     *
     * @public
     * @param {jQuery Element} $el The jQuery element.
     * @returns {Boolean} Returns true of false.
     */
    isDocOrWin: function isDocOrWin($el) {
      return $.isWindow($el[0]) || $el.is(document);
    },

    /**
     * Check if DOM element is in viewport.
     *
     * @public
     * @param {jQuery Element} options.$el The jQuery element.
     * @param {Number} options.extraSize Extra-size value to add for inViewport calculation.
     * @returns {Object} Returns an object (obj.bottom, obj.top).
     */
    isInViewport: function isInViewport(options) {
      var winDetails = this.getDetails(this.$window),
          elDetails = this.getDetails(options.$el),
          viewportTop = winDetails.position.scrollTop,
          viewportBottom = viewportTop + winDetails.size.height,
          extraSize = options.extraSize || 0,
          bottom = elDetails.position.bottom + extraSize,
          top = elDetails.position.top - extraSize;

      return {
        bottom: bottom <= viewportBottom && bottom >= viewportTop,
        top: top >= viewportTop && top <= viewportBottom
      };
    },

    /**
     * Remove key in localStorage
     *
     * @public
     * @param {String} key The key to remove.
     */
    removeStorageItem: function removeStorageItem(key) {
      if (this.hasStorage()) localStorage.removeItem(key);
    },

    /**
     * Return value rounded with Math.round() function.
     *
     * @public
     * @param {Number} value The value to round.
     * @param {Number} decimal The decimal to affect.
     * @returns {Number} Returns the value of a number rounded to the nearest integer.
     */
    roundTo: function roundTo(value, decimal) {
      decimal = decimal || 1;
      return Math.round(value * decimal) / decimal;
    },

    /**
     * Set key/value to store in localStorage.
     *
     * @public
     * @param {String} key The key to set.
     * @param {Object} value The value to set.
     */
    setStorageItem: function setStorageItem(key, value) {
      if (this.hasStorage()) localStorage.setItem(key, value);
    },

    /**
     * The method to scroll to a position.
     *
     * @public
     * @param {Number} top The scroll top position.
     * @param {Number} duration The scroll duration.
     * @param {Function} cb Call-back function when scroll ends.
     */
    scrollTo: function scrollTo(top, duration, cb) {
      $('html, body').animate({
        scrollTop: top
      }, duration, _.debounce(function () {
        if (cb) cb();
      }, 0));
    }
  });

  /***********************/
  /*** PLUGIN INSTANCE ***/
  /***********************/
  var ptJsCore = function ptJsCore() {
    if (!$.data(document, "gmksPtJsCore")) $.data(document, "gmksPtJsCore", new Plugin());
    return $.data(document, "gmksPtJsCore");
  };

  /******************************/
  /*** WINDOW OBJECT EXPOSURE ***/
  /******************************/
  window.$.ptJsCore = ptJsCore();
})(jQuery, window, document, _);
'use strict';

var _typeof = typeof Symbol === "function" && typeof Symbol.iterator === "symbol" ? function (obj) { return typeof obj; } : function (obj) { return obj && typeof Symbol === "function" && obj.constructor === Symbol && obj !== Symbol.prototype ? "symbol" : typeof obj; };

/**
 * @name          : PageTour.js Step - jQuery Widget
 * @version       : 1.1.1
 * @cat           : Plugin/Widget/Tour/Onboard/Engage/Guide
 * @author        : Gamaki Studio (Gael Mamadi Stucki)
 * @documentation : http://jquery-widget.pagetourjs.com
 * @website       : http://www.pagetourjs.com
 * @copyright     : (c) 2017 Gamaki Studio (Gael Mamadi Stucki)
 */

;(function (ptJsCore, $, _) {
  'use strict';

  /** Constant to define the list of animations. */

  var _ANIMATIONS = {
    ptJs_bounce: {
      in: 'bounceIn',
      out: 'bounceOut'
    },
    ptJs_bounceDirection: {
      in: {
        b: 'bounceInUp',
        l: 'bounceInLeft',
        r: 'bounceInRight',
        t: 'bounceInDown',
        'cb-l': 'bounceInLeft',
        'cb-r': 'bounceInRight',
        'ct-l': 'bounceInLeft',
        'ct-r': 'bounceInRight',
        'ml': 'bounceInLeft',
        'mr': 'bounceInRight',
        'cm': 'bounceInDown'
      },
      out: {
        b: 'bounceOutDown',
        l: 'bounceOutLeft',
        r: 'bounceOutRight',
        t: 'bounceOutUp',
        'cb-l': 'bounceOutLeft',
        'cb-r': 'bounceOutRight',
        'ct-l': 'bounceOutLeft',
        'ct-r': 'bounceOutRight',
        'ml': 'bounceOutLeft',
        'mr': 'bounceOutRight',
        'cm': 'bounceOutUp'
      }
    },
    ptJs_fade: {
      in: 'fadeIn',
      out: 'fadeOut'
    },
    ptJs_fadeDirection: {
      in: {
        b: 'fadeInUp',
        l: 'fadeInLeft',
        r: 'fadeInRight',
        t: 'fadeInDown',
        'cb-l': 'fadeInLeft',
        'cb-r': 'fadeInRight',
        'ct-l': 'fadeInLeft',
        'ct-r': 'fadeInRight',
        'ml': 'fadeInLeft',
        'mr': 'fadeInRight',
        'cm': 'fadeInDown'
      },
      out: {
        b: 'fadeOutDown',
        l: 'fadeOutLeft',
        r: 'fadeOutRight',
        t: 'fadeOutUp',
        'cb-l': 'fadeOutLeft',
        'cb-r': 'fadeOutRight',
        'ct-l': 'fadeOutLeft',
        'ct-r': 'fadeOutRight',
        'ml': 'fadeOutLeft',
        'mr': 'fadeOutRight',
        'cm': 'fadeOutUp'
      }
    },
    ptJs_flipDirection: {
      in: {
        b: 'flipInX',
        l: 'flipInY',
        r: 'flipInY',
        t: 'flipInX',
        'cb-l': 'flipInY',
        'cb-r': 'flipInY',
        'ct-l': 'flipInY',
        'ct-r': 'flipInY',
        'ml': 'flipInY',
        'mr': 'flipInY',
        'cm': 'flipInX'
      },
      out: {
        b: 'flipOutX',
        l: 'flipOutY',
        r: 'flipOutY',
        t: 'flipOutX',
        'cb-l': 'flipOutY',
        'cb-r': 'flipOutY',
        'ct-l': 'flipOutY',
        'ct-r': 'flipOutY',
        'ml': 'flipOutY',
        'mr': 'flipOutY',
        'cm': 'flipOutX'
      }
    },
    ptJs_lightSpeed: {
      in: 'lightSpeedIn',
      out: 'lightSpeedOut'
    },
    ptJs_roll: {
      in: 'rollIn',
      out: 'rollOut'
    },
    ptJs_rotate: {
      in: 'rotateIn',
      out: 'rotateOut'
    },
    ptJs_slideDirection: {
      in: {
        b: 'slideInUp',
        l: 'slideInLeft',
        r: 'slideInRight',
        t: 'slideInDown',
        'cb-l': 'slideInLeft',
        'cb-r': 'slideInRight',
        'ct-l': 'slideInLeft',
        'ct-r': 'slideInRight',
        'ml': 'slideInLeft',
        'mr': 'slideInRight',
        'cm': 'slideInDown'
      },
      out: {
        b: 'slideOutDown',
        l: 'slideOutLeft',
        r: 'slideOutRight',
        t: 'slideOutUp',
        'cb-l': 'slideOutLeft',
        'cb-r': 'slideOutRight',
        'ct-l': 'slideOutLeft',
        'ct-r': 'slideOutRight',
        'ml': 'slideOutLeft',
        'mr': 'slideOutRight',
        'cm': 'slideOutUp'
      }
    },
    ptJs_zoom: {
      in: 'zoomIn',
      out: 'zoomOut'
    },
    ptJs_zoomDirection: {
      in: {
        b: 'zoomInUp',
        l: 'zoomInLeft',
        r: 'zoomInRight',
        t: 'zoomInDown',
        'cb-l': 'zoomInLeft',
        'cb-r': 'zoomInRight',
        'ct-l': 'zoomInLeft',
        'ct-r': 'zoomInRight',
        'ml': 'zoomInLeft',
        'mr': 'zoomInRight',
        'cm': 'zoomInDown'
      },
      out: {
        b: 'zoomOutDown',
        l: 'zoomOutLeft',
        r: 'zoomOutRight',
        t: 'zoomOutUp',
        'cb-l': 'zoomOutLeft',
        'cb-r': 'zoomOutRight',
        'ct-l': 'zoomOutLeft',
        'ct-r': 'zoomOutRight',
        'ml': 'zoomOutLeft',
        'mr': 'zoomOutRight',
        'cm': 'zoomOutUp'
      }
    }
  };

  /** Constants to define the list of displays */
  var _DISPLAY_MARKER = 'marker';
  var _DISPLAY_WINDOW = 'window';

  /** Constants to define the list of locations  */
  var _LOCATION_AUTO = 'auto';

  var _LOCATION_CENTER_MIDDLE = 'cm';

  var _LOCATION_TOP_LEFT = 'tl';
  var _LOCATION_TOP_LEFT_BOTTOM = 'tl-b';
  var _LOCATION_TOP_LEFT_MIDDLE = 'tl-m';
  var _LOCATION_TOP_CENTER = 'tc';
  var _LOCATION_TOP_CENTER_BOTTOM = 'tc-b';
  var _LOCATION_TOP_CENTER_LEFT = 'tc-l';
  var _LOCATION_TOP_CENTER_LEFT_BOTTOM = 'tc-l-b';
  var _LOCATION_TOP_CENTER_LEFT_MIDDLE = 'tc-l-m';
  var _LOCATION_TOP_CENTER_MIDDLE = 'tc-m';
  var _LOCATION_TOP_CENTER_RIGHT = 'tc-r';
  var _LOCATION_TOP_CENTER_RIGHT_BOTTOM = 'tc-r-b';
  var _LOCATION_TOP_CENTER_RIGHT_MIDDLE = 'tc-r-m';
  var _LOCATION_TOP_RIGHT = 'tr';
  var _LOCATION_TOP_RIGHT_BOTTOM = 'tr-b';
  var _LOCATION_TOP_RIGHT_MIDDLE = 'tr-m';

  var _LOCATION_RIGHT_TOP = 'rt';
  var _LOCATION_RIGHT_TOP_CENTER = 'rt-c';
  var _LOCATION_RIGHT_MIDDLE = 'rm'; // Current
  var _LOCATION_RIGHT_MIDDLE_BOTTOM = 'rm-b';
  var _LOCATION_RIGHT_MIDDLE_BOTTOM_CENTER = 'rm-b-c';
  var _LOCATION_RIGHT_MIDDLE_BOTTOM_LEFT = 'rm-b-l';
  var _LOCATION_RIGHT_MIDDLE_CENTER = 'rm-c';
  var _LOCATION_RIGHT_MIDDLE_LEFT = 'rm-l';
  var _LOCATION_RIGHT_MIDDLE_TOP = 'rm-t';
  var _LOCATION_RIGHT_MIDDLE_TOP_CENTER = 'rm-t-c';
  var _LOCATION_RIGHT_MIDDLE_TOP_LEFT = 'rm-t-l';
  var _LOCATION_RIGHT_BOTTOM = 'rb';
  var _LOCATION_RIGHT_BOTTOM_CENTER = 'rb-c';

  var _LOCATION_BOTTOM_RIGHT = 'br';
  var _LOCATION_BOTTOM_RIGHT_MIDDLE = 'br-m';
  var _LOCATION_BOTTOM_RIGHT_TOP = 'br-t';
  var _LOCATION_BOTTOM_CENTER = 'bc';
  var _LOCATION_BOTTOM_CENTER_LEFT = 'bc-l';
  var _LOCATION_BOTTOM_CENTER_LEFT_MIDDLE = 'bc-l-m';
  var _LOCATION_BOTTOM_CENTER_LEFT_TOP = 'bc-l-t';
  var _LOCATION_BOTTOM_CENTER_MIDDLE = 'bc-m';
  var _LOCATION_BOTTOM_CENTER_RIGHT = 'bc-r';
  var _LOCATION_BOTTOM_CENTER_RIGHT_MIDDLE = 'bc-r-m';
  var _LOCATION_BOTTOM_CENTER_RIGHT_TOP = 'bc-r-t';
  var _LOCATION_BOTTOM_CENTER_TOP = 'bc-t';
  var _LOCATION_BOTTOM_LEFT = 'bl';
  var _LOCATION_BOTTOM_LEFT_MIDDLE = 'bl-m';
  var _LOCATION_BOTTOM_LEFT_TOP = 'bl-t';

  var _LOCATION_LEFT_BOTTOM = 'lb';
  var _LOCATION_LEFT_BOTTOM_CENTER = 'lb-c';
  var _LOCATION_LEFT_MIDDLE = 'lm';
  var _LOCATION_LEFT_MIDDLE_BOTTOM = 'lm-b';
  var _LOCATION_LEFT_MIDDLE_BOTTOM_CENTER = 'lm-b-c';
  var _LOCATION_LEFT_MIDDLE_BOTTOM_RIGHT = 'lm-b-r';
  var _LOCATION_LEFT_MIDDLE_CENTER = 'lm-c';
  var _LOCATION_LEFT_MIDDLE_RIGHT = 'lm-r';
  var _LOCATION_LEFT_MIDDLE_TOP = 'lm-t';
  var _LOCATION_LEFT_MIDDLE_TOP_CENTER = 'lm-t-c';
  var _LOCATION_LEFT_MIDDLE_TOP_RIGHT = 'lm-t-r';
  var _LOCATION_LEFT_TOP = 'lt';
  var _LOCATION_LEFT_TOP_CENTER = 'lt-c';

  var _LOCATION_CORNER_BOTTOM_LEFT = 'cb-l'; // Current
  var _LOCATION_CORNER_BOTTOM_LEFT_CENTER = 'cb-l-c';
  var _LOCATION_CORNER_BOTTOM_LEFT_CENTER_MIDDLE = 'cb-l-c-m';
  var _LOCATION_CORNER_BOTTOM_LEFT_MIDDLE = 'cb-l-m';
  var _LOCATION_CORNER_BOTTOM_RIGHT = 'cb-r';
  var _LOCATION_CORNER_BOTTOM_RIGHT_CENTER = 'cb-r-c';
  var _LOCATION_CORNER_BOTTOM_RIGHT_CENTER_MIDDLE = 'cb-r-c-m';
  var _LOCATION_CORNER_BOTTOM_RIGHT_MIDDLE = 'cb-r-m';
  var _LOCATION_CORNER_TOP_LEFT = 'ct-l';
  var _LOCATION_CORNER_TOP_LEFT_CENTER = 'ct-l-c';
  var _LOCATION_CORNER_TOP_LEFT_CENTER_MIDDLE = 'ct-l-c-m';
  var _LOCATION_CORNER_TOP_LEFT_MIDDLE = 'ct-l-m';
  var _LOCATION_CORNER_TOP_RIGHT = 'ct-r';
  var _LOCATION_CORNER_TOP_RIGHT_CENTER = 'ct-r-c';
  var _LOCATION_CORNER_TOP_RIGHT_CENTER_MIDDLE = 'ct-r-c-m';
  var _LOCATION_CORNER_TOP_RIGHT_MIDDLE = 'ct-r-m';

  var _LOCATION_LIST_OUT = [_LOCATION_TOP_LEFT, _LOCATION_TOP_CENTER_LEFT, _LOCATION_TOP_CENTER, _LOCATION_TOP_CENTER_RIGHT, _LOCATION_TOP_RIGHT, _LOCATION_CORNER_TOP_RIGHT_CENTER, _LOCATION_CORNER_TOP_RIGHT, _LOCATION_CORNER_TOP_RIGHT_MIDDLE, _LOCATION_RIGHT_TOP, _LOCATION_RIGHT_MIDDLE_TOP, _LOCATION_RIGHT_MIDDLE, _LOCATION_RIGHT_MIDDLE_BOTTOM, _LOCATION_RIGHT_BOTTOM, _LOCATION_CORNER_BOTTOM_RIGHT_MIDDLE, _LOCATION_CORNER_BOTTOM_RIGHT, _LOCATION_CORNER_BOTTOM_RIGHT_CENTER, _LOCATION_BOTTOM_RIGHT, _LOCATION_BOTTOM_CENTER_RIGHT, _LOCATION_BOTTOM_CENTER, _LOCATION_BOTTOM_CENTER_LEFT, _LOCATION_BOTTOM_LEFT, _LOCATION_CORNER_BOTTOM_LEFT_CENTER, _LOCATION_CORNER_BOTTOM_LEFT, _LOCATION_CORNER_BOTTOM_LEFT_MIDDLE, _LOCATION_LEFT_BOTTOM, _LOCATION_LEFT_MIDDLE_BOTTOM, _LOCATION_LEFT_MIDDLE, _LOCATION_LEFT_MIDDLE_TOP, _LOCATION_LEFT_TOP, _LOCATION_CORNER_TOP_LEFT_MIDDLE, _LOCATION_CORNER_TOP_LEFT, _LOCATION_CORNER_TOP_LEFT_CENTER];

  /** Constant to define max-width for mobile device */
  var _MOBILE_MAX_WIDTH = 768;

  /** Constants to define the list of overlays mode */
  var _OVERLAY_MODE_FOCUS = 'focus';
  var _OVERLAY_MODE_FULL = 'full';

  /** Constants for max width for mobile device */
  var _STEP_MARKER_ID_PREFIX = '_PTJS_STEP_MARKER_';
  var _STEP_ID_PREFIX = '_PTJS_STEP_';

  /** Constant to define the ID to use when creating WRAPPERS. */
  var _WRAPPER_ID = '_PTJS_WRAPPER';
  var _WRAPPER_MARKERS_ID = '_PTJS_WRAPPER_MARKERS';
  var _WRAPPER_OVERLAYS_ID = '_PTJS_WRAPPER_OVERLAYS';
  var _WRAPPER_STEPS_ID = '_PTJS_WRAPPER_STEPS';

  /** Check ptJsCore detection. */
  if (!ptJsCore) {
    console.log('!!! ptJsCore is undefined !!! This plugin is mandatory to run ptJsStep.');
    return;
  }

  /************************/
  /*** ptJsStep Widget ***/
  /************************/
  $.widget('gmks.ptJsStep', {
    version: '1.1.0',
    options: {
      animation: {
        enableOnClose: false,
        enableOnOpen: true,
        duration: 0.5,
        name: ['ptJs_fadeDirection']
      },
      autoOpen: false,
      display: {
        markerSwitchTo: _DISPLAY_WINDOW,
        mode: _DISPLAY_WINDOW
      },
      id: null,
      modal: false,
      overlay: {
        color: 'rgba(0, 0, 0, 0.50)',
        duration: 0.25,
        enable: true,
        mode: _OVERLAY_MODE_FOCUS,
        padding: 0
      },
      position: {
        location: _LOCATION_AUTO,
        locationMobile: _LOCATION_AUTO,
        relocate: false
      },
      scrollToElement: {
        enable: false,
        duration: 750
      },
      size: {
        minWidth: 200,
        resizable: true,
        width: 300
      },
      templateClass: {},
      templateData: {},
      templateStyle: {},
      templateViewMarker: '',
      templateViewWindow: '',
      theme: 'classic',
      windowMargin: 0
    },

    /***********************/
    /*** PRIVATE METHODS ***/
    /***********************/

    /**
     * Method to animate the step
     *
     * @private
     * @param {Boolean} options.out The animation direction : 'in' => start, 'out' => end.
     * @param {String || array} options.name The animation name.
     * @param {Function} options.cb Call-back function when animation ends.
     */
    _animate: function _animate(options) {
      var _this = this;

      var options = options || {};
      if (!this.options.animation.enableOnOpen && !options.out || !this.options.animation.enableOnClose && options.out) return;

      var dir = options.out ? 'out' : 'in',
          location = this.options.position.location,
          locationPrefix = location[0] === 'c' ? location.substring(0, 4) : location[0] === 'm' ? location : location[0],
          name = options.name && options.name[0] || this.options.animation.name;

      if (name.length > 0) name = name[dir === 'in' ? 0 : name.length - 1];
      if (_ANIMATIONS[name]) {
        name = _typeof(_ANIMATIONS[name][dir]) === 'object' ? _ANIMATIONS[name][dir][locationPrefix] : _ANIMATIONS[name][dir];
      }

      this._setTriggerEvent('onBeforeAnimate', { direction: dir });
      ptJsCore.animate(this._$step, name, function () {
        _this._setTriggerEvent('onAfterAnimate', { direction: dir });
        if (options.cb) options.cb();
      });
    },

    /**
     * Method to pre-initialize stuff before create method.
     *
     * @private
     */
    _beforeCreate: function _beforeCreate() {
      ptJsCore.$body = $('body');
    },

    /**
     * Method to run all binding methods.
     *
     * @private
     */
    _bindEvents: function _bindEvents() {
      this._onDocumentScroll();
      this._onWindowResize();
      this._onClick();
    },

    /**
     * The widget's constructor.
     *
     * @private
     */
    _create: function _create() {
      this._beforeCreate();
      this._createDom();
      this._bindEvents();
    },

    /**
     * Method to call all creation methods.
     *
     * @private
     */
    _createDom: function _createDom() {
      this._createDomWrapper();
      this._createDomOverlays();
      this._createDomStep();
    },

    /**
     * Method to create "overlays" DOM.
     *
     * @private
     */
    _createDomOverlays: function _createDomOverlays() {
      if ($('.ptjs-overlays').length === 0) {
        this._$overlays = $('<div>', { class: 'ptjs-overlays' }).appendTo(ptJsCore.$wrapper.overlays);
        $('<div>', { class: 'ptjs-overlay top' }).appendTo(this._$overlays);
        $('<div>', { class: 'ptjs-overlay right' }).appendTo(this._$overlays);
        $('<div>', { class: 'ptjs-overlay bottom' }).appendTo(this._$overlays);
        $('<div>', { class: 'ptjs-overlay left' }).appendTo(this._$overlays);
        $('<div>', { class: 'ptjs-overlay full' }).appendTo(this._$overlays);
      } else {
        this._$overlays = $('.ptjs-overlays');
      }
    },

    /**
     * Method to create "step" DOM.
     *
     * @private
     */
    _createDomStep: function _createDomStep() {
      this._$step = $('<div>', {
        class: 'ptjs-step' + this._getTheme()
      }).appendTo(ptJsCore.$wrapper.steps).html(this._gettemplateViewWindow());
      this._setTemplateClass(this.options.templateClass);
      this._setTemplateStyle(this.options.templateStyle);
      this._setCSS();
      this._setCSSArrow(this.options.position.location);
      this._$stepMarker = $('<div>', {
        class: 'ptjs-step-marker' + this._getTheme()
      }).appendTo(ptJsCore.$wrapper.markers).html(this._getTemplateViewMarker());
    },

    /**
     * Method to create "wrappers" DOM.
     *
     * @private
     */
    _createDomWrapper: function _createDomWrapper() {
      ptJsCore.$wrapper.core = ptJsCore.createDomWrapper(_WRAPPER_ID, ptJsCore.$body);
      ptJsCore.$wrapper.overlays = ptJsCore.createDomWrapper(_WRAPPER_OVERLAYS_ID, ptJsCore.$wrapper.core);
      ptJsCore.$wrapper.steps = ptJsCore.createDomWrapper(_WRAPPER_STEPS_ID, ptJsCore.$wrapper.core);
      ptJsCore.$wrapper.markers = ptJsCore.createDomWrapper(_WRAPPER_MARKERS_ID, ptJsCore.$wrapper.core);
    },

    /**
     * Removes the JQuery widget from the element and cleans up all common data,
     *
     * @private
     */
    _destroy: function _destroy() {
      this._$step.remove();
      this._$stepMarker.remove();
    },

    /**
     * Method to get location direction relative to the 'position location' option.
     *
     * @private
     * @returns {String} Returns direction
     */
    _getLocationDirection: function _getLocationDirection() {
      var location = this.options.position.location;
      if (location.indexOf('t') === 0 || location.indexOf('ct') === 0) {
        return 'top';
      } else if (location.indexOf('b') === 0 || location.indexOf('cb') === 0) {
        return 'bottom';
      } else if (location.indexOf('l') === 0) {
        return 'left';
      } else if (location.indexOf('r') === 0) {
        return 'right';
      }
    },

    /**
     * Method to create and get the step-guide
     *
     * @private
     * @param {Object} stepDetails The object containing step's details.
     * @returns {jQuery Element} Returns jQuery element.
     */
    _getStepGuide: function _getStepGuide(stepDetails) {
      return $('<div>', {
        class: 'ptjs-step-guide'
      }).css({
        height: stepDetails.size.fullHeight,
        width: stepDetails.size.fullWidth,
        top: stepDetails.position.top,
        left: stepDetails.position.left
      });
    },

    /**
     * Method to get template-view-marker jQuery Element
     *
     * @private
     * @returns {jQuery Element} Returns jQuery element.
     */
    _getTemplateViewMarker: function _getTemplateViewMarker() {
      this._$templateViewMarker = $(this.options.templateViewMarker);
      return this._$templateViewMarker;
    },

    /**
     * Method to get template-view-step jQuery Element
     *
     * @private
     * @returns {jQuery Element} Returns jQuery element.
     */
    _gettemplateViewWindow: function _gettemplateViewWindow() {
      this._$templateViewWindow = $(this.options.templateViewWindow);
      this._setTemplateData(this.options.templateData);
      return this._$templateViewWindow;
    },

    /**
     * Method to get theme
     *
     * @private
     * @returns {String} Returns theme.
     */
    _getTheme: function _getTheme() {
      return ' ptjs-theme ' + this.options.theme;
    },

    /**
     * Method to check if jQuery element has exceed window dimension.
     *
     * @private
     * @param {jQuery Element} $el The jQuery element.
     * @param {String} only Sets the axe to check : both || x (left,right) || y (top,bottom).
     * @returns {Boolean} Returns true of false.
     */
    _hasExceed: function _hasExceed($el, only) {
      return ptJsCore.hasExceed($el, this.options.windowMargin, only);
    },

    /**
     * Method to hide overlays
     *
     * @private
     */
    _hideOverlay: function _hideOverlay() {
      this._$overlays.hide();
      this._$overlays.removeClass('active');
      this._$overlays.find('> div').removeAttr('style');
    },

    /**
     * The widget initialization.
     *
     * @private
     */
    _init: function _init() {
      /** Sets specific location */
      if (ptJsCore.isDocOrWin(this.element) && this.options.position.location === _LOCATION_AUTO) {
        this.options.position.location = _LOCATION_CENTER_MIDDLE;
      } else if (ptJsCore.$window.width() < _MOBILE_MAX_WIDTH) {
        this.options.position.location = this.options.position.locationMobile;
      }

      /** Object to store original option */
      this._stepOriginal = {
        details: ptJsCore.getDetails(this._$step),
        options: ptJsCore.mergeOptions({}, this.options)
      };

      /** Object to store informations */
      this._stepInfo = {
        autoLocation: this._stepOriginal.options.position.location === _LOCATION_AUTO,
        currentDisplayMode: this.options.display.mode,
        isMobile: ptJsCore.$window.width() < _MOBILE_MAX_WIDTH
      };

      /** Other initializations */
      this._$step.attr('id', _STEP_ID_PREFIX + this.options.id);
      this._$stepMarker.attr('id', _STEP_MARKER_ID_PREFIX + this.options.id);
      if (this.options.autoOpen) this.open();
    },

    /**
     * Method to define click events.
     *
     * @private
     */
    _onClick: function _onClick() {
      var _this2 = this;

      this._$step.on('click', function (evt) {
        evt.stopPropagation();
      });

      /* Step 'button click' event */
      this._$step.find('[ptjs-id*="button"]').on('click', function (evt) {
        var buttonName = _.map($(evt.target).attr('ptjs-id').split('-'), function (item) {
          return _.capitalize(item);
        }).join('');

        _this2._stepInfo.lastButtonClicked = buttonName;
        _this2._setTriggerEvent('on' + buttonName + 'Click', { evt: evt });
      });

      /* Marker 'click' event */
      this._$stepMarker.on('click', function (evt) {
        evt.stopPropagation();
        _this2._stepInfo.currentDisplayMode = _this2.options.display.markerSwitchTo;
        _this2._setTriggerEvent('onMarkerClick', { evt: evt });
      });

      /* Overlay 'click' event */
      this._$overlays.on('click', function (evt) {
        evt.stopPropagation();
        if (!_this2._stepInfo.isOpen) return;
        _this2._setTriggerEvent('onOverlayClick', { evt: evt });
        if (!_this2.options.modal) _this2.close({ closeOverlay: true, kill: true });
      });
      // ptJsCore.$window.on('click', evt => {
      //   if (!this._stepInfo.isOpen) return;
      //   this._setTriggerEvent(`onWindowClick`, { evt: evt });
      //   if (!this.options.modal && this._stepInfo.windowClicked) this.close({ closeOverlay: true, kill: true });
      //   this._stepInfo.windowClicked = true;
      // });
    },

    /**
     * Method to define scroll event.
     *
     * @private
     */
    _onDocumentScroll: function _onDocumentScroll() {
      var _this3 = this;

      ptJsCore.$document.scroll(function () {
        if (!_this3._stepInfo.isOpen || _this3._scrollTopLastValue === ptJsCore.$document.scrollTop()) return; // Fix extra triggering 
        ptJsCore.$window.trigger('resize');
        _this3._scrollTopLastValue = ptJsCore.$document.scrollTop();
      });
    },

    /**
     * Method to define window resize event
     *
     * @private
     */
    _onWindowResize: function _onWindowResize() {
      var _this4 = this;

      ptJsCore.$window.on('resize', function () {
        if (_this4._stepInfo.isOpen) {
          if (_this4.options.overlay.enable) _this4._showOverlay();
          _this4._relocate();
          _this4._setPosition();
        }
        if (_this4.options.display.mode === _DISPLAY_MARKER) {
          _this4._setPosition({ stepMarker: _this4._$stepMarker });
        }
      });
    },

    /**
     * Method to open the step.
     *
     * @private
     */
    _open: function _open() {
      var _this5 = this;

      var stepDetails = ptJsCore.getDetails(this._$step),
          exec = function exec() {
        /** The step's open workflow */
        _this5._setTriggerEvent('onBeforeOpen');
        if (_this5.options.display.mode === _DISPLAY_WINDOW || _this5._stepInfo.currentDisplayMode === _DISPLAY_WINDOW) {
          _this5._$step.show();
          _this5._relocate();
          _this5._setPosition();
          if (_this5.options.animation.enableOnOpen) {
            _this5._animate({
              cb: function cb() {
                _this5._setTriggerEvent('onAfterOpen');
              }
            });
          } else {
            _this5._setTriggerEvent('onAfterOpen');
          }
        }
        _this5._stepInfo.isOpen = true;
        ptJsCore.info.lastIsDocOrWin = ptJsCore.isDocOrWin(_this5.element);
      },
          exec2 = function exec2() {
        if (_this5.options.overlay.enable) {
          _this5._showOverlay(function () {
            exec();
          });
        } else {
          _this5._hideOverlay();
          exec();
        }
      },
          locationDirection = this._getLocationDirection(),
          isInViewport = ptJsCore.isInViewport({
        $el: this.element,
        extraSize: this._stepInfo.autoLocation ? 0 : stepDetails.size.fullHeight
      });

      isInViewport = this._stepInfo.autoLocation || this.options.display.mode === _DISPLAY_MARKER || locationDirection === 'left' || locationDirection === 'right' ? isInViewport.top && isInViewport.bottom : isInViewport.top && locationDirection === 'top' || isInViewport.bottom && locationDirection === 'bottom';

      if (this.options.scrollToElement.enable && !isInViewport && !ptJsCore.isDocOrWin(this.element)) {
        this._hideOverlay();
        this._setTriggerEvent('onScrollStart');
        this._scrollToElement(function () {
          _this5._setTriggerEvent('onScrollEnd');
          exec2();
        });
      } else {
        exec2();
      }
    },

    /**
     * Method to open the marker.
     *
     * @private
     */
    _openMarker: function _openMarker() {
      this._$stepMarker.show();
      this._relocate();
      this._setPosition({ stepMarker: this._$stepMarker });
    },

    /**
     * Method to relocate step or marker while window resizing or scolling.
     *
     * @private
     */
    _relocate: function _relocate() {
      var _this6 = this;

      // TO REFACTORISE
      if (!this.options.position.relocate && (!this._stepInfo.autoLocation || this._stepInfo.autoLocation && this._stepInfo.isOpen) || ptJsCore.isDocOrWin(this.element)) return;

      var stepDetails = ptJsCore.getDetails(this._$step),
          $stepGuide = this._getStepGuide(stepDetails);

      /** Check if step exceed window dimension */
      if (this._hasExceed(this._$step) || this._stepInfo.autoLocation) {
        var elDetails = ptJsCore.getDetails(this.element),
            docDetails = ptJsCore.getDetails(ptJsCore.$window),
            stepReachMinWidth = stepDetails.size.width <= this.options.size.minWidth,
            stepReachMinHeight = stepDetails.size.height <= this.options.size.minHeight,
            availableLocation = [];

        // Loop over available locations (_LOCATION_LIST_OUT) to find to next valid step's location. 
        $stepGuide.prependTo(ptJsCore.$wrapper.steps);
        _LOCATION_LIST_OUT.forEach(function (location, idx) {
          $stepGuide.css({
            height: _this6._stepOriginal.details.size.fullHeight,
            width: _this6._stepOriginal.details.size.fullWidth
          });
          _this6._setPosition({ stepGuide: $stepGuide, dynLocation: location, stopAfterPosition: true });

          var gapstepDetails = ptJsCore.getDetails($stepGuide),
              total = Math.abs(gapstepDetails.position.middle - (docDetails.position.middle + docDetails.position.scrollTop)) + Math.abs(gapstepDetails.position.center - docDetails.position.center);

          availableLocation.push({
            location: location,
            total: total,
            exceed: _this6._hasExceed($stepGuide)
          });
        });
        $stepGuide.remove();

        if (availableLocation.length === 0) return;
        var restartFromIdx = _.findIndex(availableLocation, function (item) {
          return item.location === _this6.options.position.location;
        }),
            start = availableLocation.slice(restartFromIdx),
            end = availableLocation.slice(0, restartFromIdx),
            newAvailableLocation = start.concat(end);

        newAvailableLocation = _.filter(newAvailableLocation, function (item) {
          return !item.exceed;
        });
        if (this._stepInfo.autoLocation) newAvailableLocation = _.orderBy(newAvailableLocation, ['total']);
        availableLocation = newAvailableLocation;

        if (availableLocation.length === 0) return;
        if (!this._stepInfo.autoLocation) {
          this._stepLocationWorkflow = this._stepLocationWorkflow || [];
          this._stepLocationWorkflow.push({
            location: this.options.position.location,
            width: stepReachMinWidth ? stepDetails.size.fullWidth : null,
            height: stepReachMinHeight ? stepDetails.size.fullHeight : null
          });
        }

        this.options.position.location = availableLocation[0].location;
        this._$step.css({ right: 'auto', bottom: 'auto' });
      } else {
        /** Restore the default step's location */
        if (this._stepLocationWorkflow && this._stepLocationWorkflow.length > 0) {
          var lastStepLocationWorkflow = this._stepLocationWorkflow[this._stepLocationWorkflow.length - 1];

          if (lastStepLocationWorkflow.width) $stepGuide.css('width', lastStepLocationWorkflow.width);
          if (lastStepLocationWorkflow.height) $stepGuide.css('height', lastStepLocationWorkflow.height);
          $stepGuide.prependTo(ptJsCore.$wrapper.steps);
          this._setPosition({ stepGuide: $stepGuide, dynLocation: lastStepLocationWorkflow.location, stopAfterPosition: true });

          if (!this._hasExceed($stepGuide)) {
            this.options.position.location = lastStepLocationWorkflow.location;
            this._stepLocationWorkflow.pop();
            this._setPosition({ stopAfterPosition: true });
          }
          $stepGuide.remove();
        }
      }
    },

    /**
     * Method to resize step while window resizing.
     *
     * @private
     */
    _resize: function _resize() {
      var exceed = ptJsCore.getExceedPosition(this._$step, this.options.windowMargin),
          stepDetails = ptJsCore.getDetails(this._$step),
          docDetails = ptJsCore.getDetails(ptJsCore.$document),
          winDetails = ptJsCore.getDetails(ptJsCore.$window),
          elDetails = ptJsCore.getDetails(this.element),
          location = this.options.position.location,
          css = {};

      if (this.options.size.resizable) {
        if (stepDetails.size.width > this._stepOriginal.details.size.width) {
          css.left = this._$step.position.left;
          css.right = '';
          css.width = this.options.size.width;
          this._stepInfo.isResizingLeft = false;
        } else {
          if (exceed.left || this._stepInfo.isResizingLeft) {
            css.width = 'auto';
            css.left = this.options.windowMargin;
            exceed.left = this._stepInfo.isResizingLeft = true;
          }
          if (exceed.right) {
            css.width = 'auto';
            css.right = this.options.windowMargin;
          }

          if ([_LOCATION_CORNER_BOTTOM_LEFT_CENTER, _LOCATION_CORNER_BOTTOM_LEFT_CENTER_MIDDLE, _LOCATION_CORNER_TOP_LEFT_CENTER, _LOCATION_CORNER_TOP_LEFT_CENTER_MIDDLE, _LOCATION_LEFT_MIDDLE_CENTER, _LOCATION_LEFT_MIDDLE_BOTTOM_CENTER, _LOCATION_LEFT_MIDDLE_TOP_CENTER, _LOCATION_LEFT_BOTTOM_CENTER, _LOCATION_LEFT_TOP_CENTER].indexOf(location) !== -1) {
            if (exceed.left) css.right = winDetails.size.width - (elDetails.position.left + stepDetails.size.fullWidth / 2);
          } else if ([_LOCATION_CORNER_BOTTOM_LEFT, _LOCATION_CORNER_BOTTOM_LEFT_MIDDLE, _LOCATION_CORNER_TOP_LEFT, _LOCATION_CORNER_TOP_LEFT_MIDDLE, _LOCATION_LEFT_BOTTOM, _LOCATION_LEFT_MIDDLE, _LOCATION_LEFT_MIDDLE_BOTTOM, _LOCATION_LEFT_MIDDLE_TOP, _LOCATION_LEFT_TOP].indexOf(location) !== -1) {
            if (exceed.left) css.right = winDetails.size.width - elDetails.position.left;
          } else if ([_LOCATION_BOTTOM_RIGHT, _LOCATION_TOP_RIGHT, _LOCATION_TOP_RIGHT_BOTTOM, _LOCATION_TOP_RIGHT_MIDDLE].indexOf(location) !== -1) {
            if (exceed.left) css.right = winDetails.size.width - elDetails.position.right;
          } else if ([_LOCATION_BOTTOM_CENTER, _LOCATION_TOP_CENTER, _LOCATION_TOP_CENTER_BOTTOM, _LOCATION_TOP_CENTER_MIDDLE].indexOf(location) !== -1) {
            if (exceed.left) css.right = winDetails.size.width - (elDetails.position.center + stepDetails.size.fullWidth / 2);
          } else if ([_LOCATION_BOTTOM_CENTER_LEFT_MIDDLE, _LOCATION_BOTTOM_CENTER_LEFT, _LOCATION_TOP_CENTER_LEFT, _LOCATION_TOP_CENTER_LEFT_MIDDLE].indexOf(location) !== -1) {
            if (exceed.left) css.right = winDetails.size.width - elDetails.position.center;
          }
        }
      }

      if (!$.isEmptyObject(css)) {
        this._$step.css(css);
        return true;
      }
    },

    /**
     * Method to scroll to element.
     * 
     * @private
     * @param {Function} cb Call-back function when scroll ends.
     */
    _scrollToElement: function _scrollToElement(cb) {
      var winDetails = ptJsCore.getDetails(ptJsCore.$window),
          elDetails = ptJsCore.getDetails(this.element),
          stepDetails = ptJsCore.getDetails(this._$step),
          gapElViewport = winDetails.size.height - elDetails.size.fullHeight,
          top = elDetails.position.top - gapElViewport / 2,
          preScrollBottom = elDetails.position.top + winDetails.size.height;

      if (this._getLocationDirection() === 'top') {
        var exceedTop = top - (stepDetails.size.fullHeight + this.options.windowMargin - gapElViewport / 2);
        if (exceedTop < top) top = exceedTop;
      } else if (this._getLocationDirection() === 'bottom') {
        var exceedBottom = top + elDetails.size.fullHeight + stepDetails.size.fullHeight + this.options.windowMargin + gapElViewport;
        if (exceedBottom > preScrollBottom) top = top + (exceedBottom - preScrollBottom);
      }
      if (top !== ptJsCore.info.lastScrollToElementTop) {
        ptJsCore.scrollTo(top, this.options.scrollToElement.duration, cb);
      } else cb();
      ptJsCore.info.lastScrollToElementTop = top;
    },

    /**
     * Method to set CSS to step's arrow
     *
     * @private
     * @param {String} location Step's location
     */
    _setCSSArrow: function _setCSSArrow(location) {
      if (_LOCATION_LIST_OUT.indexOf(location) === -1 || ptJsCore.isDocOrWin(this.element)) return;

      this._$step.removeClass(function (index, className) {
        return (className.match(/(^|\s)step-arrow-\S+/g) || []).join(' ');
      }).addClass('step-arrow-' + this._getLocationDirection()).addClass('step-arrow-' + location);
    },

    /**
     * Method to set CSS to step
     *
     * @private
     */
    _setCSS: function _setCSS() {
      var duration = this.options.animation.duration.toString() + 's';
      this._$step.css({
        'min-width': this.options.size.minWidth,
        width: this.options.size.width,
        'webkit-animation-duration': duration,
        'animation-duration': duration
      });
      if (ptJsCore.isDocOrWin(this.element)) this._$step.css('position', 'fixed');
    },

    /**
     * Called from the _setOptions() method for each individual option
     *
     * @private
     * @param {String} key The name of the option to set.
     * @param {Object} value A value to set for the option.
     */
    _setOption: function _setOption(key, value) {
      var prevOptions = this.options;
      this._super(key, _.isObject(value) ? ptJsCore.mergeOptions(this.options[key], value) : value);

      switch (key) {
        case 'animation':
          this._setCSS();
          break;
        case 'position':
          this._stepOriginal.options.position.location = value.location;
          this._stepInfo.autoLocation = this._stepOriginal.options.position.location === _LOCATION_AUTO;
          this._relocate();
          this._setPosition();
          this._animate();
          break;
        case 'size':
          this._stepOriginal.details.size.width = value.width;
          this._setCSS();
          this._relocate();
          this._setPosition();
          break;
        case 'theme':
          this._$step.removeClass(prevOptions.theme).addClass(value);
          break;
        case 'templateData':
          this._setTemplateData(value);
          break;
        case 'templateClass':
          this._setTemplateClass(value);
          break;
        case 'templateStyle':
          this._setTemplateStyle(value);
          break;
        case 'templateView':
          this._$step.html(this._getView());
          this._relocate();
          this._setPosition();
          this._bindEvents();
          break;
      }
    },

    /**
     * Called whenever the option() method is called
     *
     * @private
     * @param {Object} options An object containing options to set, with the name of the option as the key and the option value as the value.
     */
    _setOptions: function _setOptions(options) {
      this._super(options);
    },

    /**
     * Method to position step at the beginning or while window resizing or scrolling.
     *
     * @private
     * @param {jQuery Element} options.stepGuide The 'step-guide' jQuery element.
     * @param {jQuery Element} options.stepMarker The 'step-marker' jQuery element.
     * @param {String} options.dynLocation A specific location
     * @param {Boolean} options.stopAfterPosition If 'true' process stop after position and the resize method is not invoke.
     */
    _setPosition: function _setPosition(options) {
      var elDetails = ptJsCore.getDetails(this.element, this.options.overlay.padding),
          options = options || {},
          step = options.stepGuide || options.stepMarker || this._$step,
          stepDetails = ptJsCore.getDetails(step),
          exceed = ptJsCore.getExceedPosition(step, this.options.windowMargin),
          location = options.dynLocation || this.options.position.location,
          stepTop = elDetails.position.middle - stepDetails.size.fullHeight / 2,
          stepLeft = elDetails.position.center - stepDetails.size.fullWidth / 2,
          css = {};

      if (!options.stepGuide && !options.stepMarker) {
        this._setCSSArrow(location);
        stepDetails = ptJsCore.getDetails(step);
      }

      if (ptJsCore.isDocOrWin(this.element)) {
        step.css({
          top: 'auto',
          left: 'auto',
          right: 'auto',
          bottom: 'auto',
          'margin-left': 'auto',
          'margin-top': 'auto'
        });

        // Top
        if ([_LOCATION_TOP_LEFT, _LOCATION_TOP_CENTER, _LOCATION_TOP_RIGHT].indexOf(location) !== -1) {
          css.top = this.options.windowMargin;
        } else if ([_LOCATION_BOTTOM_LEFT, _LOCATION_BOTTOM_CENTER, _LOCATION_BOTTOM_RIGHT].indexOf(location) !== -1) {
          css.bottom = this.options.windowMargin;
        } else {
          css.top = '50%';
          css['margin-top'] = -stepDetails.size.fullHeight / 2;
        }

        // Left
        if ([_LOCATION_TOP_LEFT, _LOCATION_LEFT_MIDDLE, _LOCATION_BOTTOM_LEFT].indexOf(location) !== -1) {
          css.left = this.options.windowMargin;
        } else if ([_LOCATION_TOP_RIGHT, _LOCATION_RIGHT_MIDDLE, _LOCATION_BOTTOM_RIGHT].indexOf(location) !== -1) {
          css.right = this.options.windowMargin;
        } else {
          css.left = '50%';
          css['margin-left'] = -stepDetails.size.fullWidth / 2;
        }

        step.css(css);
      } else {
        // Top
        if ([_LOCATION_TOP_LEFT, _LOCATION_CORNER_TOP_LEFT_CENTER, _LOCATION_CORNER_TOP_LEFT, _LOCATION_TOP_CENTER, _LOCATION_TOP_CENTER_LEFT, _LOCATION_TOP_CENTER_RIGHT, _LOCATION_TOP_RIGHT, _LOCATION_CORNER_TOP_RIGHT_CENTER, _LOCATION_CORNER_TOP_RIGHT].indexOf(location) !== -1) {
          stepTop = elDetails.position.top - stepDetails.size.fullHeight;
        } else if ([_LOCATION_TOP_LEFT_MIDDLE, _LOCATION_CORNER_TOP_LEFT_CENTER_MIDDLE, _LOCATION_TOP_CENTER_MIDDLE, _LOCATION_TOP_CENTER_LEFT_MIDDLE, _LOCATION_TOP_CENTER_RIGHT_MIDDLE, _LOCATION_TOP_RIGHT_MIDDLE, _LOCATION_CORNER_TOP_RIGHT_CENTER_MIDDLE, _LOCATION_CORNER_TOP_RIGHT_MIDDLE, _LOCATION_CORNER_TOP_LEFT_MIDDLE].indexOf(location) !== -1) {
          stepTop = elDetails.position.top - stepDetails.size.fullHeight / 2;
        } else if ([_LOCATION_TOP_LEFT_BOTTOM, _LOCATION_LEFT_TOP_CENTER, _LOCATION_TOP_CENTER_BOTTOM, _LOCATION_TOP_CENTER_LEFT_BOTTOM, _LOCATION_TOP_CENTER_RIGHT_BOTTOM, _LOCATION_TOP_RIGHT_BOTTOM, _LOCATION_RIGHT_TOP_CENTER, _LOCATION_RIGHT_TOP, _LOCATION_LEFT_TOP].indexOf(location) !== -1) {
          stepTop = elDetails.position.top;
        } else if ([_LOCATION_RIGHT_MIDDLE, _LOCATION_RIGHT_MIDDLE_CENTER, _LOCATION_RIGHT_MIDDLE_LEFT, _LOCATION_LEFT_MIDDLE, _LOCATION_LEFT_MIDDLE_CENTER, _LOCATION_LEFT_MIDDLE_RIGHT].indexOf(location) !== -1) {
          stepTop = elDetails.position.middle - stepDetails.size.fullHeight / 2;
        } else if ([_LOCATION_RIGHT_MIDDLE_BOTTOM, _LOCATION_RIGHT_MIDDLE_BOTTOM_CENTER, _LOCATION_RIGHT_MIDDLE_BOTTOM_LEFT, _LOCATION_LEFT_MIDDLE_BOTTOM, _LOCATION_LEFT_MIDDLE_BOTTOM_CENTER, _LOCATION_LEFT_MIDDLE_BOTTOM_RIGHT].indexOf(location) !== -1) {
          stepTop = elDetails.position.middle;
        } else if ([_LOCATION_RIGHT_MIDDLE_TOP, _LOCATION_RIGHT_MIDDLE_TOP_CENTER, _LOCATION_RIGHT_MIDDLE_TOP_LEFT, _LOCATION_LEFT_MIDDLE_TOP, _LOCATION_LEFT_MIDDLE_TOP_CENTER, _LOCATION_LEFT_MIDDLE_TOP_RIGHT].indexOf(location) !== -1) {
          stepTop = elDetails.position.middle - stepDetails.size.fullHeight;
        } else if ([_LOCATION_RIGHT_BOTTOM, _LOCATION_BOTTOM_RIGHT_TOP, _LOCATION_RIGHT_BOTTOM_CENTER, _LOCATION_BOTTOM_CENTER_TOP, _LOCATION_BOTTOM_CENTER_LEFT_TOP, _LOCATION_BOTTOM_CENTER_RIGHT_TOP, _LOCATION_BOTTOM_LEFT_TOP, _LOCATION_LEFT_BOTTOM_CENTER, _LOCATION_LEFT_BOTTOM].indexOf(location) !== -1) {
          stepTop = elDetails.position.bottom - stepDetails.size.fullHeight;
        } else if ([_LOCATION_CORNER_BOTTOM_RIGHT_MIDDLE, _LOCATION_BOTTOM_RIGHT_MIDDLE, _LOCATION_CORNER_BOTTOM_RIGHT_CENTER_MIDDLE, _LOCATION_BOTTOM_CENTER_MIDDLE, _LOCATION_BOTTOM_CENTER_LEFT_MIDDLE, _LOCATION_BOTTOM_CENTER_RIGHT_MIDDLE, _LOCATION_BOTTOM_LEFT_MIDDLE, _LOCATION_CORNER_BOTTOM_LEFT_CENTER_MIDDLE, _LOCATION_CORNER_BOTTOM_LEFT_MIDDLE].indexOf(location) !== -1) {
          stepTop = elDetails.position.bottom - stepDetails.size.fullHeight / 2;
        } else if ([_LOCATION_BOTTOM_RIGHT, _LOCATION_CORNER_BOTTOM_RIGHT_CENTER, _LOCATION_CORNER_BOTTOM_RIGHT, _LOCATION_BOTTOM_CENTER, _LOCATION_BOTTOM_CENTER_LEFT, _LOCATION_BOTTOM_CENTER_RIGHT, _LOCATION_BOTTOM_LEFT, _LOCATION_CORNER_BOTTOM_LEFT_CENTER, _LOCATION_CORNER_BOTTOM_LEFT].indexOf(location) !== -1) {
          stepTop = elDetails.position.bottom;
        }

        // Left
        if ([_LOCATION_TOP_LEFT, _LOCATION_TOP_LEFT_MIDDLE, _LOCATION_TOP_LEFT_BOTTOM].indexOf(location) !== -1) {
          stepLeft = elDetails.position.left;
        } else if ([_LOCATION_CORNER_TOP_LEFT_CENTER, _LOCATION_CORNER_TOP_LEFT_CENTER_MIDDLE, _LOCATION_LEFT_TOP_CENTER, _LOCATION_CORNER_BOTTOM_LEFT_CENTER, _LOCATION_CORNER_BOTTOM_LEFT_CENTER_MIDDLE, _LOCATION_LEFT_BOTTOM_CENTER, _LOCATION_LEFT_MIDDLE_CENTER, _LOCATION_LEFT_MIDDLE_TOP_CENTER, _LOCATION_LEFT_MIDDLE_BOTTOM_CENTER].indexOf(location) !== -1) {
          stepLeft = elDetails.position.left - stepDetails.size.fullWidth / 2;
        } else if ([_LOCATION_CORNER_TOP_LEFT, _LOCATION_CORNER_BOTTOM_LEFT, _LOCATION_LEFT_BOTTOM, _LOCATION_CORNER_BOTTOM_LEFT_MIDDLE, _LOCATION_LEFT_MIDDLE, _LOCATION_LEFT_MIDDLE_TOP, _LOCATION_LEFT_MIDDLE_BOTTOM, _LOCATION_LEFT_TOP, _LOCATION_CORNER_TOP_LEFT_MIDDLE].indexOf(location) !== -1) {
          stepLeft = elDetails.position.left - stepDetails.size.fullWidth;
        } else if ([_LOCATION_TOP_CENTER, _LOCATION_TOP_CENTER_MIDDLE, _LOCATION_TOP_CENTER_BOTTOM, _LOCATION_BOTTOM_CENTER, _LOCATION_BOTTOM_CENTER_MIDDLE, _LOCATION_BOTTOM_CENTER_TOP].indexOf(location) !== -1) {
          stepLeft = elDetails.position.center - stepDetails.size.fullWidth / 2;
        } else if ([_LOCATION_TOP_CENTER_LEFT, _LOCATION_TOP_CENTER_LEFT_MIDDLE, _LOCATION_TOP_CENTER_LEFT_BOTTOM, _LOCATION_BOTTOM_CENTER_LEFT, _LOCATION_BOTTOM_CENTER_LEFT_MIDDLE, _LOCATION_BOTTOM_CENTER_LEFT_TOP].indexOf(location) !== -1) {
          stepLeft = elDetails.position.center - stepDetails.size.fullWidth;
        } else if ([_LOCATION_TOP_CENTER_RIGHT, _LOCATION_TOP_CENTER_RIGHT_MIDDLE, _LOCATION_TOP_CENTER_RIGHT_BOTTOM, _LOCATION_BOTTOM_CENTER_RIGHT, _LOCATION_BOTTOM_CENTER_RIGHT_MIDDLE, _LOCATION_BOTTOM_CENTER_RIGHT_TOP].indexOf(location) !== -1) {
          stepLeft = elDetails.position.center;
        } else if ([_LOCATION_TOP_RIGHT, _LOCATION_TOP_RIGHT_MIDDLE, _LOCATION_TOP_RIGHT_BOTTOM, _LOCATION_RIGHT_MIDDLE_LEFT, _LOCATION_RIGHT_MIDDLE_TOP_LEFT, _LOCATION_RIGHT_MIDDLE_BOTTOM_LEFT, _LOCATION_BOTTOM_RIGHT, _LOCATION_BOTTOM_RIGHT_MIDDLE, _LOCATION_BOTTOM_RIGHT_TOP].indexOf(location) !== -1) {
          stepLeft = elDetails.position.right - stepDetails.size.fullWidth;
        } else if ([_LOCATION_CORNER_TOP_RIGHT_CENTER, _LOCATION_CORNER_TOP_RIGHT_CENTER_MIDDLE, _LOCATION_RIGHT_TOP_CENTER, _LOCATION_RIGHT_MIDDLE_CENTER, _LOCATION_RIGHT_MIDDLE_TOP_CENTER, _LOCATION_RIGHT_MIDDLE_BOTTOM_CENTER, _LOCATION_CORNER_BOTTOM_RIGHT_CENTER, _LOCATION_CORNER_BOTTOM_RIGHT_CENTER_MIDDLE, _LOCATION_RIGHT_BOTTOM_CENTER].indexOf(location) !== -1) {
          stepLeft = elDetails.position.right - stepDetails.size.fullWidth / 2;
        } else if ([_LOCATION_RIGHT_TOP, _LOCATION_CORNER_TOP_RIGHT_MIDDLE, _LOCATION_RIGHT_MIDDLE, _LOCATION_RIGHT_MIDDLE_TOP, _LOCATION_RIGHT_MIDDLE_BOTTOM, _LOCATION_RIGHT_BOTTOM, _LOCATION_CORNER_BOTTOM_RIGHT_MIDDLE, _LOCATION_CORNER_BOTTOM_RIGHT, _LOCATION_CORNER_TOP_RIGHT].indexOf(location) !== -1) {
          stepLeft = elDetails.position.right;
        } else if ([_LOCATION_BOTTOM_LEFT, _LOCATION_BOTTOM_LEFT_MIDDLE, _LOCATION_BOTTOM_LEFT_TOP, _LOCATION_LEFT_MIDDLE_RIGHT, _LOCATION_LEFT_MIDDLE_TOP_RIGHT, _LOCATION_LEFT_MIDDLE_BOTTOM_RIGHT].indexOf(location) !== -1) {
          stepLeft = elDetails.position.left;
        }

        // Position
        step.css({ left: stepLeft, top: stepTop });

        // Resize
        if (!options.stopAfterPosition && !options.stepGuide && !options.stepMarker && this.options.size.resizable && this._resize()) {
          this._setPosition({ stopAfterPosition: true });
        }
      }
    },

    /**
     * Method to inject data into the template-view (step, step-continue and marker).
     *
     * @private
     * @param {Object} data An object containing template-data
     */
    _setTemplateData: function _setTemplateData(data) {
      var _this7 = this;

      _.keys(data).forEach(function (key) {
        _this7._$templateViewWindow.find('[ptjs-data="' + key + '"]').html(data[key]);
        _this7._$templateViewWindow.find('[ptjs-src="' + key + '"]').attr('src', data[key]);
      });
    },

    /**
     * Method to inject class into the template-view (step, step-continue and marker).
     *
     * @private
     * @param {Object} elements An object containing template-class
     */
    _setTemplateClass: function _setTemplateClass(elements) {
      var _this8 = this;

      _.keys(elements).forEach(function (key) {
        var $el = key === 'root' ? _this8._$templateViewWindow.parent() : key === 'container' ? _this8._$templateViewWindow : _this8._$templateViewWindow.find('[ptjs-id="' + key + '"]'),
            currentClass = $el.attr('class') ? $el.attr('class').split(' ') : [],
            newClass = _.concat(currentClass, elements[key]);

        $el.removeAttr('class').attr('class', newClass.join(' '));
      });
    },

    /**
     * Method to inject style into the template-view (step, step-continue and marker).
     *
     * @private
     * @param {Object} elements An object containing template-style
     */
    _setTemplateStyle: function _setTemplateStyle(elements) {
      var _this9 = this;

      _.keys(elements).forEach(function (key) {
        var $el = key === 'root' ? _this9._$templateViewWindow.parent() : key === 'container' ? _this9._$templateViewWindow : _this9._$templateViewWindow.find('[ptjs-id="' + key + '"]');
        $el.css(elements[key]);
      });
    },

    /**
     * Method to encapsulate the jQuery widget _trigger() method.
     *
     * @private
     * @param {String} name The name of the triggered event.
     * @param {Object} data The object to pass the this triggered event.
     */
    _setTriggerEvent: function _setTriggerEvent(name, data) {
      var data = ptJsCore.mergeOptions(this.info(), data);
      this._trigger(name, null, data);
    },

    /**
     * Method to show overlays
     *
     * @private
     * @param {Function} cb Call-back function when overlays animation ends.
     */
    _showOverlay: function _showOverlay(cb) {
      var elDetails = ptJsCore.getDetails(this.element),
          $overlayTop = this._$overlays.find('.top'),
          $overlayRight = this._$overlays.find('.right'),
          $overlayBottom = this._$overlays.find('.bottom'),
          $overlayLeft = this._$overlays.find('.left'),
          $overlayFull = this._$overlays.find('.full'),
          prevNextIsSameDocOrWin = ptJsCore.info.lastIsDocOrWin === ptJsCore.isDocOrWin(this.element),
          duration = prevNextIsSameDocOrWin ? this.options.overlay.duration : 0,
          animation = 'all ' + duration + 's linear',
          mobileHeightFix = this._stepInfo.isMobile ? 100 : 0,
          cssTop = {
        left: elDetails.position.left - this.options.overlay.padding,
        top: 0,
        height: elDetails.position.top - this.options.overlay.padding - ptJsCore.$document.scrollTop(),
        width: ptJsCore.$window.width() - elDetails.position.left + this.options.overlay.padding,
        'background-color': this.options.overlay.color,
        transition: animation,
        '-webkit-transition': animation
      },
          cssRight = {
        left: elDetails.position.right + this.options.overlay.padding,
        top: elDetails.position.top - this.options.overlay.padding - ptJsCore.$document.scrollTop(),
        height: ptJsCore.$window.height() - elDetails.position.top + this.options.overlay.padding + ptJsCore.$document.scrollTop() + mobileHeightFix,
        width: ptJsCore.$window.width() - elDetails.position.right - this.options.overlay.padding,
        'background-color': this.options.overlay.color,
        transition: animation,
        '-webkit-transition': animation
      },
          cssBottom = {
        left: 0,
        top: elDetails.position.bottom + this.options.overlay.padding - ptJsCore.$document.scrollTop(),
        height: ptJsCore.$window.height() - elDetails.position.bottom - this.options.overlay.padding + ptJsCore.$document.scrollTop() + mobileHeightFix,
        width: elDetails.position.right + this.options.overlay.padding,
        'background-color': this.options.overlay.color,
        transition: animation,
        '-webkit-transition': animation
      },
          cssLeft = {
        top: 0,
        left: 0,
        height: elDetails.position.bottom + this.options.overlay.padding - ptJsCore.$document.scrollTop(),
        width: elDetails.position.left - this.options.overlay.padding,
        'background-color': this.options.overlay.color,
        transition: animation,
        '-webkit-transition': animation
      };

      if (cb) {
        this._$overlays.show();
      } else {
        cssTop['transition'] = '';
        cssTop['-webkit-transition'] = '';
        cssRight['transition'] = '';
        cssRight['-webkit-transition'] = '';
        cssBottom['transition'] = '';
        cssBottom['-webkit-transition'] = '';
        cssLeft['transition'] = '';
        cssLeft['-webkit-transition'] = '';
      }

      if (this.options.overlay.mode === _OVERLAY_MODE_FOCUS && !ptJsCore.isDocOrWin(this.element)) {
        var transitionEnd = 'webkitTransitionEnd otransitionend oTransitionEnd msTransitionEnd transitionend',
            fired = false,
            samePos = ptJsCore.roundTo($overlayTop.width()) === ptJsCore.roundTo(cssTop.width) && ptJsCore.roundTo($overlayBottom.width()) === ptJsCore.roundTo(cssBottom.width) && ptJsCore.roundTo($overlayRight.width()) === ptJsCore.roundTo(cssRight.width) && ptJsCore.roundTo($overlayLeft.width()) === ptJsCore.roundTo(cssLeft.width) && ptJsCore.roundTo($overlayTop.height()) === ptJsCore.roundTo(cssTop.height) && ptJsCore.roundTo($overlayBottom.height()) === ptJsCore.roundTo(cssBottom.height) && ptJsCore.roundTo($overlayRight.height()) === ptJsCore.roundTo(cssRight.height) && ptJsCore.roundTo($overlayLeft.height()) === ptJsCore.roundTo(cssLeft.height);

        $overlayTop.css(cssTop);
        $overlayBottom.css(cssBottom);
        $overlayRight.css(cssRight);
        $overlayLeft.off(transitionEnd).one(transitionEnd, function () {
          if (cb && !fired) {
            fired = true;
            cb();
          }
        }).css(cssLeft);
        if ((duration === 0 || samePos || this._$overlays.hasClass('full')) && cb) cb();
        this._$overlays.addClass('active').removeClass('full');
      } else {
        this._$overlays.addClass('full').removeClass('active');
        $overlayFull.css({ top: 0, left: 0, right: 0, bottom: 0, 'background-color': this.options.overlay.color });
        if (cb) cb();
      }
    },
    /**********************/
    /*** PUBLIC METHODS ***/
    /**********************/

    /**
      * Method to close the step.
      *
      * @public
      * @param {Boolean} options.kill Propagate this value to the trigerred event.
      * @param {jQuery Element} options.closeOverlay If 'true', close the overlay.
      */
    close: function close(options) {
      var _this10 = this;

      if (!this._stepInfo.isOpen) return;

      var options = options || {},
          data = options.kill ? { kill: true } : {};

      this._stepInfo.isOpen = false;
      this._stepInfo.windowClicked = false;
      this._stepInfo.currentDisplayMode = this.options.display.mode;

      if (this.options.animation.enableOnClose) {
        this._setTriggerEvent('onBeforeClose', data);
        this._animate({
          out: true,
          cb: function cb() {
            _this10._$step.hide();
            if (options.closeOverlay) _this10._hideOverlay();
            _this10._setTriggerEvent('onAfterClose', data);
          }
        });
      } else {
        this._setTriggerEvent('onBeforeClose', data);
        this._$step.hide();
        if (options.closeOverlay) this._hideOverlay();
        this._setTriggerEvent('onAfterClose', data);
      }
    },

    /**
     * Method to get info about a the step.
     *
     * @public
     * @returns {Object} Returns step's info.
     */
    info: function info() {
      return {
        $element: this.element,
        isOpen: this._stepInfo.isOpen,
        lastButtonClicked: this._stepInfo.lastButtonClicked,
        $step: this._$step,
        $stepMarker: this._$stepMarker
      };
    },

    /**
     * Method to open the step.
     *
     * @public
     */
    open: function open() {
      if (this._stepInfo.isOpen) return;
      if (this.options.display.mode === _DISPLAY_MARKER && this._stepInfo.currentDisplayMode === _DISPLAY_MARKER) this._openMarker();else this._open();
    }
  });

  /******************************/
  /*** WINDOW OBJECT EXPOSURE ***/
  /******************************/
  window.$.ptJsStep = $.gmks.ptJsStep;
})($.ptJsCore, jQuery, _);
'use strict';

/**
 * @name          : PageTour.js - jQuery Widget
 * @version       : 1.1.1
 * @cat           : Plugin/Widget/Tour/Onboard/Engage/Guide
 * @author        : Gamaki Studio (Gael Mamadi Stucki)
 * @documentation : http://jquery-widget.pagetourjs.com
 * @website       : http://www.pagetourjs.com
 * @copyright     : (c) 2017 Gamaki Studio (Gael Mamadi Stucki)
 */

;(function (ptJsCore, ptJsStep, ptJsThemes, $, _) {
  'use strict';

  /** Constants to define the list of displays */

  var _DISPLAY_MARKER = 'marker';
  var _DISPLAY_WINDOW = 'window';

  /** Constant to define the widget prefix name. */
  var _PTJS_STEP_WIDGET_NAME = 'ptJsStep';

  /** Constants to define the list of mode runs */
  var _RUN_MODE_MANUAL = 'manual';
  var _RUN_MODE_AUTO = 'auto';

  /** Constants to define the list of localStorage keys */
  var _STORAGE_KEY_STEP_AUTOSTART_ONCE = 'step-autostart-once';
  var _STORAGE_KEY_STEP_CONTINUE_INDEX = 'step-continue-index';

  /** Check ptJsCore & ptJjStep detection */
  if (!ptJsCore || !ptJsStep) {
    console.log('!!! ptJsCore and/or ptJsStep is/are undefined !!! Those plugins are mandatory to run ptJs.');
    return;
  }

  /********************/
  /*** ptJs Widget ***/
  /*******************/
  $.widget('gmks.ptJs', {
    version: '1.1.0',
    options: {
      animation: {
        enableOnClose: false,
        enableOnOpen: true,
        duration: 0.5,
        name: ['ptJs_fadeDirection']
      },
      autoDestroy: true,
      autoStart: false,
      autoStartOnce: false,
      continueEnable: false,
      display: {
        markerSwitchTo: _DISPLAY_WINDOW,
        markerShowOnce: false,
        mode: _DISPLAY_WINDOW
      },
      keyboardNavigation: true,
      instanceName: '',
      modal: false,
      overlay: {
        color: 'rgba(0, 0, 0, 0.50)',
        duration: 0.25,
        enable: true,
        mode: 'focus',
        padding: 5
      },
      position: {
        location: 'auto',
        locationMobile: 'auto',
        relocate: false
      },
      scrollDuration: 750,
      showButtonClose: true,
      size: {
        minWidth: 150,
        resizable: true,
        width: 300
      },
      steps: [],
      startAt: 0,
      templateClass: {},
      templateData: {
        'button-close': 'x',
        'button-continue': 'continue',
        'button-end': 'end',
        'button-next': 'next',
        'button-previous': 'previous',
        'button-restart': 'restart',
        'button-start': 'start',
        content: 'PageTour.js: Makes your website easier and guides your users',
        'content-continue': 'Click "continue" to continue the tour or click "restart" to restart the tour from the beginning.',
        title: 'PageTour.js',
        'title-continue': 'Continue the Tour ?'
      },
      templateStyle: {},
      templateViewMarker: '',
      templateViewContinue: '',
      templateViewWindow: '',
      theme: 'classic',
      windowMargin: 20
    },

    /***********************/
    /*** PRIVATE METHODS ***/
    /***********************/

    /**
     * Method to pre-initialize stuff before create method.
     *
     * @private
     */
    _beforeCreate: function _beforeCreate() {
      var _this = this;

      this.options.steps = _.filter(this.options.steps, function (step) {
        return ptJsCore.domExist(step.el);
      });

      var elementStepOccurences = _.chain(this.options.steps).groupBy(function (item) {
        return item.el;
      }).mapValues(function (item) {
        return item.length;
      }).pickBy(function (val) {
        return val > 1;
      }).valueOf(),
          occurencesIdx = [];

      this._elementStepsWidgetName = [];
      this.options.steps.forEach(function (step, idx) {
        if (elementStepOccurences[step.el]) {
          if (!occurencesIdx[step.el]) occurencesIdx[step.el] = [];
          _this._elementStepsWidgetName[idx] = _PTJS_STEP_WIDGET_NAME + _this._getInstanceName() + '_' + occurencesIdx[step.el].length;
          occurencesIdx[step.el].push('.');
        } else {
          _this._elementStepsWidgetName[idx] = _PTJS_STEP_WIDGET_NAME + _this._getInstanceName();
        }
      });
    },

    /**
     * Method to run all binding methods.
     *
     * @private
     */
    _bindEvents: function _bindEvents() {
      if (this.options.keyboardNavigation) this._onKey();
    },

    /**
     * The widget's constructor.
     *
     * @private
     */
    _create: function _create() {
      if (!this.options.templateViewContinue) this._setOption('templateViewContinue', ptJsThemes[this.options.theme].templateViewContinue);
      if (!this.options.templateViewMarker) this._setOption('templateViewMarker', ptJsThemes[this.options.theme].templateViewMarker);
      if (!this.options.templateViewWindow) this._setOption('templateViewWindow', ptJsThemes[this.options.theme].templateViewWindow);

      this._info = {
        currentStepIdx: 0,
        STORAGE_KEY_STEP_AUTOSTART_ONCE: _STORAGE_KEY_STEP_AUTOSTART_ONCE + this._getInstanceName(),
        STORAGE_KEY_STEP_CONTINUE_INDEX: _STORAGE_KEY_STEP_CONTINUE_INDEX + this._getInstanceName()
      };
      this._beforeCreate();
      this._createSteps();
      if (this.options.continueEnable) this._createStepContinue();
      this._bindEvents();
    },

    /**
     * Method to create a step
     *
     * @private
     * @param {Object} step The step's object.
     * @param {Number} stepDetails The step's index.
     * @param {Number} stepDetails The total number of steps.
     * @returns {jQuery Element} Returns jQuery element.
     */
    _createStep: function _createStep(step, idx, total) {
      var _this2 = this;

      var templateClass = {};
      if (this.options.display.mode === _DISPLAY_WINDOW) {
        templateClass['container'] = ['ptjs-display-step'];
        templateClass['pagination'] = ['ptjs-active'];
        if (idx === 0 && total > 0) templateClass['button-start'] = ['ptjs-active'];
        if (idx > 0 && idx <= total) templateClass['button-previous'] = ['ptjs-active'];
        if (idx > 0 && idx < total) templateClass['button-next'] = ['ptjs-active'];
        if (idx === total) templateClass['button-end'] = ['ptjs-active'];
        if (this.options.showButtonClose) templateClass['button-close'] = ['ptjs-active'];
      } else {
        templateClass = {
          'button-close': ['ptjs-active'],
          'container': ['ptjs-display-marker']
        };
      }

      templateClass = ptJsCore.mergeOptions(this.options.templateClass, templateClass);
      templateClass = ptJsCore.mergeOptions(templateClass, step.templateClass);

      if (this._elementStepsWidgetName[idx] !== 'ptJsStep') $.widget('gmks.' + this._elementStepsWidgetName[idx], $.gmks.ptJsStep, {});
      var newStep = $(step.el)[this._elementStepsWidgetName[idx]]({
        animation: ptJsCore.mergeOptions(this.options.animation, step.animation),
        autoOpen: false,
        display: this.options.display,
        id: idx,
        modal: this.options.display.mode !== _DISPLAY_MARKER && (step.modal || this.options.modal),
        overlay: ptJsCore.mergeOptions(this.options.overlay, step.overlay),
        position: step.position || this.options.position,
        scrollToElement: {
          enable: true,
          duration: this.options.scrollDuration
        },
        size: step.size || this.options.size,
        templateData: ptJsCore.mergeOptions(this.options.templateData, step.templateData),
        templateClass: templateClass,
        templateStyle: ptJsCore.mergeOptions(this.options.templateStyle, step.templateStyle),
        templateViewMarker: step.templateViewMarker || this.options.templateViewMarker,
        templateViewWindow: step.templateViewWindow || this.options.templateViewWindow,
        theme: step.theme || this.options.theme,
        windowMargin: this.options.windowMargin,
        onAfterAnimate: function onAfterAnimate(event, data) {
          _this2._setTriggerEvent('onAfterAnimate');
        },
        onAfterClose: function onAfterClose(event, data) {
          _this2._setTriggerEvent('onAfterClose');
          if (data.kill) data.lastButtonClicked = null;
          switch (_this2._info.lastButtonClicked || data.lastButtonClicked) {
            case 'ButtonStart':
            case 'ButtonNext':
              _this2._displayStep('next');
              break;
            case 'ButtonPrevious':
              _this2._displayStep('previous');
              break;
            default:
              _this2._setTriggerEvent('onDestroy');
              if (_this2.options.autoDestroy) $(document)['ptJs' + _this2._getInstanceName()]('destroy');
          }
          _this2._info.lastButtonClicked = null;
          if (_this2.options.display.mode === _DISPLAY_MARKER && !_this2.options.display.markerShowOnce) data.$stepMarker.show();
        },
        onAfterOpen: function onAfterOpen(event, data) {
          _this2._setTriggerEvent('onAfterOpen');
        },
        onBeforeAnimate: function onBeforeAnimate(event, data) {
          _this2._setTriggerEvent('onBeforeAnimate');
        },
        onBeforeClose: function onBeforeClose(event, data) {
          _this2._setTriggerEvent('onBeforeClose');
        },
        onBeforeOpen: function onBeforeOpen(event, data) {
          if (_this2.options.display.mode === _DISPLAY_WINDOW) {
            $(event.target)[_this2._elementStepsWidgetName[idx]]('option', 'templateData', {
              'pagination': _this2.info().index + 1 + '/' + _this2.info().total
            });
            if (_this2.options.continueEnable) ptJsCore.setStorageItem(_this2._info.STORAGE_KEY_STEP_CONTINUE_INDEX, _this2.info().index);
          }
          _this2._info.isStepContinue = false;
          _this2._setTriggerEvent('onBeforeOpen');
        },
        onButtonCloseClick: function onButtonCloseClick(event, data) {
          _this2._setTriggerEvent('onButtonCloseClick', { evt: data.evt });
          $(event.target)[_this2._elementStepsWidgetName[idx]]('close', { closeOverlay: true });
        },
        onButtonEndClick: function onButtonEndClick(event, data) {
          _this2._setTriggerEvent('onButtonEndClick', { evt: data.evt });
          $(event.target)[_this2._elementStepsWidgetName[idx]]('close', { closeOverlay: true });
          ptJsCore.removeStorageItem(_this2._info.STORAGE_KEY_STEP_CONTINUE_INDEX);
        },
        onButtonNextClick: function onButtonNextClick(event, data) {
          _this2._setTriggerEvent('onButtonNextClick', { evt: data.evt });
          $(event.target)[_this2._elementStepsWidgetName[idx]]('close');
        },
        onButtonPreviousClick: function onButtonPreviousClick(event, data) {
          _this2._setTriggerEvent('onButtonPreviousClick', { evt: data.evt });
          $(event.target)[_this2._elementStepsWidgetName[idx]]('close');
        },
        onButtonStartClick: function onButtonStartClick(event, data) {
          _this2._setTriggerEvent('onButtonStartClick', { evt: data.evt });
          $(event.target)[_this2._elementStepsWidgetName[idx]]('close');
        },
        onMarkerClick: function onMarkerClick(event, data) {
          _this2._info.currentStepIdx = _this2._getIndexByStepID(data.$step.attr('id'));
          data.$stepMarker.hide();
          $(event.target)[_this2._elementStepsWidgetName[_this2._info.currentStepIdx]]('open');
          _this2._setTriggerEvent('onMarkerClick', { evt: data.evt });
        },
        onOverlayClick: function onOverlayClick(event, data) {
          _this2._setTriggerEvent('onOverlayClick', { evt: data.evt });
        },
        onScrollEnd: function onScrollEnd(event, data) {
          _this2._setTriggerEvent('onScrollEnd');
        },
        onScrollStart: function onScrollStart(event, data) {
          _this2._setTriggerEvent('onScrollStart');
        },
        onWindowClick: function onWindowClick(event, data) {
          _this2._setTriggerEvent('onWindowClick', { evt: data.evt });
        }
      });

      // Attach other click event
      var defaultButtons = '[ptjs-id!="button-start"][ptjs-id!="button-previous"][ptjs-id!="button-next"][ptjs-id!="button-end"][ptjs-id!="button-close"]',
          customButtons = newStep[this._elementStepsWidgetName[idx]]('info').$step.find('[ptjs-id*="button"]' + defaultButtons);

      customButtons.each(function (idx, item) {
        var buttonName = _.map($(item).attr('ptjs-id').split('-'), function (item) {
          return _.capitalize(item);
        }).join(''),
            eventName = 'ptjsstepon' + buttonName.toLowerCase() + 'click';

        newStep.off(eventName).on(eventName, function (event, data) {
          _this2._setTriggerEvent('on' + buttonName + 'Click', { evt: data.evt });
        });
      });
      return newStep;
    },

    /**
     * Method to create a step-continue
     *
     * @private
     * @param {Object} step The step-continue's object.
     */
    _createStepContinue: function _createStepContinue(step) {
      var _this3 = this;

      var templateClass = {
        'button-continue': ['ptjs-active'],
        'button-restart': ['ptjs-active']
      },
          position = this.options.position;
      position.location = 'cm';

      if (this.options.showButtonClose) templateClass['button-close'] = ['ptjs-active'];

      $.widget('gmks.ptJsStepContinue' + this._getInstanceName(), $.gmks.ptJsStep, {});
      this._stepContinue = $(document)['ptJsStepContinue' + this._getInstanceName()]({
        animation: this.options.animation,
        autoOpen: false,
        id: this.options.steps.length,
        modal: this.options.modal,
        overlay: this.options.overlay,
        position: position,
        size: this.options.size,
        templateData: this.options.templateData,
        templateClass: ptJsCore.mergeOptions(templateClass, this.options.templateClass),
        templateStyle: this.options.templateStyle,
        templateViewWindow: this.options.templateViewContinue,
        theme: this.options.theme,
        onAfterAnimate: function onAfterAnimate(event, data) {
          _this3._setTriggerEvent('onAfterAnimate');
        },
        onAfterClose: function onAfterClose(event, data) {
          if (data.lastButtonClicked && data.lastButtonClicked !== 'ButtonClose') {
            _this3._setTriggerEvent('onAfterClose');
            _this3._displayStep();
          } else if (_this3.options.autoDestroy) {
            _this3._setTriggerEvent('onDestroy');
            $(document)['ptJs' + _this3._getInstanceName()]('destroy');
          }
        },
        onAfterOpen: function onAfterOpen(event, data) {
          _this3._setTriggerEvent('onAfterOpen');
        },
        onBeforeAnimate: function onBeforeAnimate(event, data) {
          _this3._setTriggerEvent('onBeforeAnimate');
        },
        onBeforeClose: function onBeforeClose(event, data) {
          _this3._setTriggerEvent('onBeforeClose');
        },
        onBeforeOpen: function onBeforeOpen(event, data) {
          _this3._info.isStepContinue = true;
          _this3._setTriggerEvent('onBeforeOpen');
        },
        onButtonCloseClick: function onButtonCloseClick(event, data) {
          _this3._setTriggerEvent('onButtonCloseClick', { evt: data.evt });
          $(event.target)['ptJsStepContinue' + _this3._getInstanceName()]('close', { closeOverlay: true });
        },
        onButtonContinueClick: function onButtonContinueClick(event, data) {
          _this3._info.currentStepIdx = parseInt(ptJsCore.getStorageItem(_this3._info.STORAGE_KEY_STEP_CONTINUE_INDEX));
          _this3._setTriggerEvent('onButtonContinueClick', { evt: data.evt });
          $(event.target)['ptJsStepContinue' + _this3._getInstanceName()]('close');
        },
        onButtonRestartClick: function onButtonRestartClick(event, data) {
          _this3._info.currentStepIdx = 0;
          ptJsCore.removeStorageItem(_this3._info.STORAGE_KEY_STEP_CONTINUE_INDEX);
          $(event.target)['ptJsStepContinue' + _this3._getInstanceName()]('close', { evt: data.evt });
        },
        onOverlayClick: function onOverlayClick(event, data) {
          _this3._setTriggerEvent('onOverlayClick', { evt: data.evt });
        },
        onWindowClick: function onWindowClick(event, data) {
          _this3._setTriggerEvent('onWindowClick', { evt: data.evt });
        }
      });
    },

    /**
     * Method to create all steps.
     *
     * @private
     */
    _createSteps: function _createSteps() {
      var _this4 = this;

      this._steps = [];
      this.options.steps.forEach(function (step, idx) {
        _this4._steps.push(_this4._createStep(step, idx, _this4.options.steps.length - 1));
      });
    },

    /**
     * Removes the JQuery widget from the element and cleans up all common data,
     *
     * @private
     */
    _destroy: function _destroy() {
      var _this5 = this;

      this._steps.forEach(function (step, idx) {
        step[_this5._elementStepsWidgetName[idx]]('destroy');
      });
      try {
        $(document)['ptJsStepContinue' + this._getInstanceName()]('destroy');
      } catch (err) {}
      if (ptJsCore.$wrapper.steps.find('.ptjs-step').length === 0) ptJsCore.$wrapper.core.remove();
    },

    /**
    * Method to display a step.
    *
    * @private
    * @param {String} dir Direction of the navigation (previous or next)
    */
    _displayStep: function _displayStep(dir) {
      if (dir) {
        if (dir === 'next') this._info.currentStepIdx++;else this._info.currentStepIdx--;
      }
      this._getCurrentStep()[this._getCurrentElementStepWidgetName()]('open');
    },

    /**
     * Method to display a step-continue.
     *
     * @private
     */
    _displayStepContinue: function _displayStepContinue() {
      this._stepContinue['ptJsStepContinue' + this._getInstanceName()]('open');
    },

    /**
     * Method to display all step's marker.
     *
     * @private
     */
    _displaySteps: function _displaySteps() {
      var _this6 = this;

      this._steps.forEach(function (step, idx) {
        step[_this6._elementStepsWidgetName[idx]]('open');
      });
    },

    /**
     * Method to get step's index relative to its ID.
     *
     * @private
     * @param {String} id The step's ID.
     * @returns {Number} Returns the step's index.
     */
    _getIndexByStepID: function _getIndexByStepID(id) {
      var _this7 = this;

      return _.findIndex(this._steps, function (step, idx) {
        return id === step[_this7._elementStepsWidgetName[idx]]('info').$step.attr('id');
      });
    },

    /**
     * Method to get current step jQuery widget.
     *
     * @private
     * @returns {jQuery Widget} Returns jQuery widget
     */
    _getCurrentStep: function _getCurrentStep() {
      return this._steps[this._info.currentStepIdx];
    },

    /**
     * Method to get current element jQuery widget name.
     *
     * @private
     * @returns {String} Returns jQuery widget name
     */
    _getCurrentElementStepWidgetName: function _getCurrentElementStepWidgetName() {
      return this._elementStepsWidgetName[this._info.currentStepIdx];
    },

    /**
     * Method to get instance name.
     *
     * @private
     * @returns {String} Returns instance name.
     */
    _getInstanceName: function _getInstanceName() {
      return this.options.instanceName ? '-' + this.options.instanceName : '';
    },

    /**
     * Method helper
     *
     * @private
     * @returns {Object} Returns object helper.
     */
    _helper: function _helper() {
      var _this8 = this;

      return {
        isStepOpened: function isStepOpened() {
          return _.find(_this8._steps, function (step, idx) {
            return step[_this8._elementStepsWidgetName[idx]]('info').isOpen;
          });
        }
      };
    },

    /**
     * The widget initialization.
     *
     * @private
     */
    _init: function _init() {
      if (this.options.autoStart && (!this.options.autoStartOnce || this.options.autoStartOnce && !ptJsCore.getStorageItem(this._info.STORAGE_KEY_STEP_AUTOSTART_ONCE))) this.start();
      if (this.options.autoStart && this.options.autoStartOnce) ptJsCore.setStorageItem(this._info.STORAGE_KEY_STEP_AUTOSTART_ONCE, true);
    },

    /**
     * Method to define key events.
     *
     * @private
     */
    _onKey: function _onKey() {
      var self = this;
      $(document).off('keydown').keydown(function (event) {
        switch (event.which) {
          case 37:
          case 38:
            if (self.info().index > 0) self.previous();
            break;
          case 39:
          case 40:
            if (!self.info().isLast) self.next();else self.end();
            break;
        }
      });
    },

    /**
     * Called from the _setOptions() method for each individual option
     *
     * @private
     * @param {String} key The name of the option to set.
     * @param {Object} value A value to set for the option.
     */
    _setOption: function _setOption(key, value) {
      this._super(key, _.isObject(value) ? ptJsCore.mergeOptions(this.options[key], value) : value);
    },

    /**
     * Called whenever the option() method is called
     *
     * @private
     * @param {Object} options An object containing options to set, with the name of the option as the key and the option value as the value.
     */
    _setOptions: function _setOptions(options) {
      this._super(options);
    },

    /**
     * Method to encapsulate the jQuery widget '_trigger()' method.
     *
     * @private
     * @param {String} name The name of the triggered event.
     * @param {Object} data The object to pass the this triggered event.
     */
    _setTriggerEvent: function _setTriggerEvent(name, data) {
      var data = ptJsCore.mergeOptions(this.info(), data);
      this._trigger(name, null, data);
    },

    /**
     * Method to start the tour.
     *
     * @private
     */
    _start: function _start() {
      this._setTriggerEvent('onStart');
      if (this.options.display.mode === _DISPLAY_MARKER) {
        this._displaySteps();
      } else {
        var spIndex = parseInt(ptJsCore.getStorageItem(this._info.STORAGE_KEY_STEP_CONTINUE_INDEX));
        if (this.options.continueEnable && spIndex > 0) {
          this._displayStepContinue();
        } else {
          this._info.currentStepIdx = this.options.startAt;
          this._displayStep();
        }
      }
    },

    /**********************/
    /*** PUBLIC METHODS ***/
    /**********************/

    /**
     * Method to end the tour.
     *
     * @public
     */
    end: function end() {
      this._getCurrentStep()[this._getCurrentElementStepWidgetName()]('close', { closeOverlay: true, kill: true });
    },

    /**
      * Method to get info about the tour.
      *
      * @public
      * @returns {Object} Returns step's info.
      */
    info: function info() {
      return {
        index: this._info.currentStepIdx,
        isLast: this._info.currentStepIdx === this._steps.length - 1,
        isStepContinue: this._info.isStepContinue,
        step: this._info.isStepContinue ? this._stepContinue : this._getCurrentStep(),
        steps: this._steps,
        total: this._steps.length
      };
    },

    /**
     * Method to go to the next step.
     *
     * @public
     */
    next: function next() {
      this._info.lastButtonClicked = 'ButtonNext';
      this._getCurrentStep()[this._getCurrentElementStepWidgetName()]('close');
    },

    /**
     * Method to go to the previous step.
     *
     * @public
     */
    previous: function previous() {
      this._info.lastButtonClicked = 'ButtonPrevious';
      this._getCurrentStep()[this._getCurrentElementStepWidgetName()]('close');
    },

    /**
     * Method to start the tour.
     *
     * @public
     */
    start: function start() {
      if (this._helper().isStepOpened()) ;
      this._start();
    },

    /**
     * Method to reset localStorage.
     *
     * @public
     */
    resetStorage: function resetStorage() {
      ptJsCore.removeStorageItem(this._info.STORAGE_KEY_STEP_CONTINUE_INDEX);
      ptJsCore.removeStorageItem(this._info.STORAGE_KEY_STEP_AUTOSTART_ONCE);
    }

  });

  /******************************/
  /*** WINDOW OBJECT EXPOSURE ***/
  /******************************/
  window.$.ptJs = function (options, instanceName) {
    var instanceName = options.instanceName || instanceName;
    instanceName = instanceName ? 'ptJs-' + instanceName : 'ptJs';

    if (options.instanceName) $.widget('gmks.ptJs-' + options.instanceName, $.gmks.ptJs, {});
    try {
      return $(document)[instanceName](options);
    } catch (err) {}
  };
})($.ptJsCore, $.ptJsStep, $.ptJsThemes, jQuery, _);