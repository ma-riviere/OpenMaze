using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.Statistics;
using MathNet.Numerics.RootFinding;
using System.Linq;

namespace utils 
{
    public class Mapping : NotifyPropertyChange {

        private double _x0;
        public double x0 { get => _x0; set => SetField(ref _x0, value); }
        private double _y0;
        public double y0 { get => _y0; set => SetField(ref _y0, value); }
        private double _x1;
        public double x1 { get => _x1; set => SetField(ref _x1, value); }
        private double _y1;
        public double y1 { get => _y1; set => SetField(ref _y1, value); }
        private LinkType _linkType;
        public LinkType linkType { get => _linkType; set => SetField(ref _linkType, value); }
        public delegate double linkFn(double x, double x0, double y0, double x1, double y1);
        private linkFn _link = PowerLink; //TODO: update on change of LinkType
        private bool _discrete;
        public bool discrete { get => _discrete; set => SetField(ref _discrete, value); }
        private int _steps;
        public int steps { get => _steps; set => SetField(ref _steps, value); }
        private bool _center;
        public bool center { get => _center; set => SetField(ref _center, value); }
        private string _stepAlong;
        public string stepAlong { get => _stepAlong; set => SetField(ref _stepAlong, value); }
        private List<double> _breaks;
        private List<double> _midpoints;

        public Mapping(double x0, double y0, double x1, double y1, LinkType linkType, bool discrete, int steps, bool center, string stepAlong)
        {
            this._x0 = x0;
            this._y0 = y0;
            this._x1 = x1;
            this._y1 = y1;
            this._linkType = linkType;
            this._discrete = discrete;
            this._steps = this._discrete ? steps : 0;
            this._center = center;
            this._stepAlong = stepAlong;

            UpdateBreaks();
        }

        public enum LinkType { Linear, Power };

        public static double LinearLink(double x, double x0, double y0, double x1, double y1)
        {
            return(y0 + (x - x0) * (y1 - y0) / (x1 - x0));
        }

        public static double PowerLink(double x, double x0, double y0, double x1, double y1)
        {
            double a = (Math.Log(y1) - Math.Log(y0)) / (Math.Log(x1) - Math.Log(x0));
            double b = Math.Log(y0) - a * Math.Log(x0);
            return(Math.Exp(a * Math.Log(x) + b));
        }

        // TODO: trigger with PropertyChangedEventArgs(propertyName)
        public void UpdateBreaks() 
        {
            if (_discrete) {
                if(_stepAlong == "x") {
                    List<double> equal_breaks_x = new List<double>(Generate.LinearSpaced(_steps + 1, _x0, _x1)); // IEnumerable<double> ?
                    this._breaks = (List<double>)equal_breaks_x.Select(x => _GetY(x));
                } else {
                    List<double> equal_breaks_y = new List<double>(Generate.LinearSpaced(_steps + 1, _y0, _y1)); // IEnumerable<double> ?
                    // Func<double, double> diff = x => _GetY(x) - i;
                    List<double> gradient_breaks_x = new List<double>(equal_breaks_y.Select(y => Bisection.FindRoot((x) => _GetY(x) - y, Math.Min(_x0, _x1), Math.Max(_x0, _x1), 1e-2, 100)));
                    this._breaks = (List<double>)gradient_breaks_x.Select(x => _GetY(x));
                }
                
                if(!_center) this._midpoints = _breaks;
                else {
                    double[] midpoints = new double[_breaks.Capacity - 1];
                    for(int i = 0; i < midpoints.Length; i++) midpoints[i] = 0.5f * (_breaks[i] + _breaks[i + 1]);
                    this._midpoints = new List<double>(midpoints);
                }
            }
        }

        /**
        *   Uses the Inverse Empirical CDF to find n breakpoints (@param _steps) in the xrange of a function constrained by equal spacing of their y values (@param data)
        *   FIXME: make it a List<double>
        */
        private double[] GetQuantiles(double[] data) 
        {
            double[] seq = Generate.LinearSpaced(_steps + 1, _x0, _x1);
            double[] quantiles = new double[_steps + 1];
            for(int i = 0; i < seq.Length; i++) quantiles[i] = Statistics.Quantile(data, seq[i]);
            return(quantiles);
        }

        /**
        *   Generates samples from a function on a restricted xrange (@params _x0 and _x1) and uses it to find equally space breakpoints
        *   FIXME: make it a List<double>
        */
        private double[] GetQuantileBreaks() 
        {
            int nSamples = 500;
            double[] xrange = Generate.LinearRange(Math.Min(_x0, _x1), Math.Abs(_x1 - _x0)/nSamples, Math.Max(_x0, _x1));
            double[] yrange = new double[xrange.Length];
            for(int i = 0; i < xrange.Length; i++) yrange[i] = _GetY(xrange[i]); // TODO: for loop / LINQ since the function only takes one value
            return GetQuantiles(yrange);
        }

        private double _GetY(double x) 
        {
            if(x <= _x0) return(_y0);
            else if(x >= _x1) return(_y1);
            else return(_link(x, _x0, _y0, _x1, _y1));
        }

        public double GetY(double x) 
        {
            if(_discrete) return(Discretize(_GetY(x)));
            else return(_GetY(x));
        }
        
        private double Discretize(double y) 
        {
            double yMin = _breaks.Min();
            double yMax = _breaks.Max();

            if (y <= yMin) return(yMin);
            else if (y >= yMax) return(yMax);
            else {
                double res = 0.0;
                for(int i = 0; i < _breaks.Capacity - 1; i++) {
                    if( y > Math.Min(_breaks[i], _breaks[i + 1]) && y <= Math.Max(_breaks[i], _breaks[i + 1]) ) {
                        if(y <= _midpoints[i]) res = Math.Min(_breaks[i], _breaks[i + 1]);
                        else res = Math.Max(_breaks[i], _breaks[i + 1]);
                    }
                }
                return(res);
            }
        }
    }
}