
; Core

; (append [1 2] [3 4]) -> [1 2 3 4]
(def append (fn [coll1 coll2]
	(if (empty? coll2)
		coll1
		(append (conj coll1 (first coll2)) (rest coll2)))))

(defmacro defn [name arglist & body]
	(list 'def name (append (list 'fn arglist) body)))

(defn second [coll] (first (rest coll)))

(defn inc [x] (+ x 1))
(defn dec [x] (- x 1))

(defn true? [x] (if x true false))
(defn not [x] (if x false true))

(defn fact [x]
	(if (= x 1)
		x
		(* x (fact (dec x)))))

(defn reverse [coll]
	(if (= null coll)
		[]
		(conj (reverse (rest coll)) (first coll))))

(defn map [f coll]
	(if (empty? coll)
		[]
		(cons (f (first coll)) (map f (rest coll)))))

(defn reduce [f result coll]
	(if (empty? coll)
		result
		(let [new-result (f result (first coll))]
			(reduce f new-result (rest coll)))))

(defn range [nr]
	(if (<= nr 0)
		[]
		(conj (range (- nr 1)) (- nr 1))))

(def create []
	(let [counter 0]
		(fn [] (inc counter))))

(defn even? [x] (= 0 (mod x 2)))
(defn odd? [x] (not (even? x)))


(defmacro do [& body]
	(cons 'let (cons [] body)))

(defmacro swap [a b]
	(list b a))

(defmacro infix [a operator b]
	(list operator a b))

(defn create-counter [start-value] 
	(let [counter start-value]
		(fn [] (set! counter (inc counter)))))

(defn time [] (invoke-static 'System.DateTime 'Now))

(defmacro test [expression]
	(list 'if expression 
		(list) 
		(list 'print "Test failed")))

(defn wrap-functions [start-value forms]
	(if (empty? forms)
		start-value
		(cons (first forms) (list (wrap-functions start-value (rest forms))))))

(defmacro -> [start-value & forms]
	(wrap-functions start-value (reverse forms)))

(defn blob [x]
	(print "Got " x))

(def t (new 'SharpLisp.Tester 100))

